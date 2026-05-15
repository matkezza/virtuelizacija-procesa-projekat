using System;
using System.Globalization;
using System.ServiceModel;
using Common;

namespace ServiceHostApp
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PmsmService : IPmsmService, IDisposable
    {
        public delegate void TransferStartedHandler(SessionMeta meta);
        public delegate void SampleReceivedHandler(int sampleNumber, MotorSample sample);
        public delegate void TransferCompletedHandler(int acceptedSamplesCount);
        public delegate void WarningRaisedHandler(string warningType, string direction, string message);

        public event TransferStartedHandler OnTransferStarted;
        public event SampleReceivedHandler OnSampleReceived;
        public event TransferCompletedHandler OnTransferCompleted;
        public event WarningRaisedHandler OnWarningRaised;

        private readonly ConfigurationReader configuration;
        private readonly SampleValidator validator;
        private SessionFileStorage storage;
        private bool sessionStarted;
        private bool disposed;
        private int acceptedSamplesCount;
        private MotorSample previousSample;
        private double speedSum;

        public PmsmService()
        {
            configuration = new ConfigurationReader();
            validator = new SampleValidator();
            SubscribeDefaultHandlers();
        }

        public SessionResponse StartSession(SessionMeta meta)
        {
            ThrowIfDisposed();
            ValidateMeta(meta);
            DisposeStorageIfExists();

            storage = new SessionFileStorage(configuration.StoragePath);
            sessionStarted = true;
            acceptedSamplesCount = 0;
            previousSample = null;
            speedSum = 0;

            storage.WriteLog("StartSession. Source=" + meta.SourceFileName + ", ExpectedRows=" + meta.ExpectedRows);
            OnTransferStarted?.Invoke(meta);

            return new SessionResponse(AckType.ACK, SessionStatus.IN_PROGRESS, "Sesija je pokrenuta.");
        }

        public SessionResponse PushSample(MotorSample sample)
        {
            ThrowIfDisposed();
            EnsureSessionStarted();

            if (sample == null)
            {
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Sample je null.", "MotorSample", 0),
                    "Sample je null.");
            }

            string validationError = validator.GetValidationError(sample);
            if (validationError != null)
            {
                storage.WriteRejectedSample(sample, validationError);
                return new SessionResponse(AckType.NACK, SessionStatus.IN_PROGRESS, validationError);
            }

            AnalyzeSample(sample);

            storage.WriteAcceptedSample(sample);
            acceptedSamplesCount++;
            speedSum += sample.Motor_Speed;
            previousSample = sample;

            OnSampleReceived?.Invoke(acceptedSamplesCount, sample);

            return new SessionResponse(AckType.ACK, SessionStatus.IN_PROGRESS, "Sample prihvacen.");
        }

        public SessionResponse EndSession()
        {
            ThrowIfDisposed();

            if (!sessionStarted)
            {
                return new SessionResponse(AckType.NACK, SessionStatus.COMPLETED, "Sesija nije bila pokrenuta.");
            }

            storage.WriteLog("EndSession. AcceptedSamples=" + acceptedSamplesCount);
            OnTransferCompleted?.Invoke(acceptedSamplesCount);

            DisposeStorageIfExists();
            sessionStarted = false;

            return new SessionResponse(AckType.ACK, SessionStatus.COMPLETED, "Sesija je zavrsena.");
        }

        private void AnalyzeSample(MotorSample sample)
        {
            if (previousSample != null)
            {
                double deltaUq = sample.U_q - previousSample.U_q;
                if (Math.Abs(deltaUq) > configuration.UqThreshold)
                {
                    RaiseWarning("VoltageSpikeQ", GetDirection(deltaUq), "DeltaUq=" + deltaUq.ToString(CultureInfo.InvariantCulture));
                }

                double deltaUd = sample.U_d - previousSample.U_d;
                if (Math.Abs(deltaUd) > configuration.UdThreshold)
                {
                    RaiseWarning("VoltageSpikeD", GetDirection(deltaUd), "DeltaUd=" + deltaUd.ToString(CultureInfo.InvariantCulture));
                }

                double deltaSpeed = sample.Motor_Speed - previousSample.Motor_Speed;
                if (Math.Abs(deltaSpeed) > configuration.SpeedThreshold)
                {
                    RaiseWarning("SpeedSpike", GetDirection(deltaSpeed), "DeltaSpeed=" + deltaSpeed.ToString(CultureInfo.InvariantCulture));
                }
            }

            if (acceptedSamplesCount > 0)
            {
                double currentMean = speedSum / acceptedSamplesCount;
                double lowerLimit = currentMean * (1 - configuration.AllowedDeviation);
                double upperLimit = currentMean * (1 + configuration.AllowedDeviation);

                if (sample.Motor_Speed < lowerLimit)
                {
                    RaiseWarning("OutOfBandWarning", "ispod ocekivane vrednosti", "Speed=" + sample.Motor_Speed.ToString(CultureInfo.InvariantCulture) + ", Speed_mean=" + currentMean.ToString(CultureInfo.InvariantCulture));
                }
                else if (sample.Motor_Speed > upperLimit)
                {
                    RaiseWarning("OutOfBandWarning", "iznad ocekivane vrednosti", "Speed=" + sample.Motor_Speed.ToString(CultureInfo.InvariantCulture) + ", Speed_mean=" + currentMean.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        private void RaiseWarning(string warningType, string direction, string message)
        {
            storage.WriteLog("WARNING " + warningType + " | " + direction + " | " + message);
            OnWarningRaised?.Invoke(warningType, direction, message);
        }

        private string GetDirection(double delta)
        {
            return delta > 0 ? "iznad ocekivanog" : "ispod ocekivanog";
        }

        private void ValidateMeta(SessionMeta meta)
        {
            if (meta == null)
            {
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Meta-zaglavlje nije poslato.", "meta", 0),
                    "Meta-zaglavlje nije poslato.");
            }

            RequireMetaField(meta.U_q, "U_q");
            RequireMetaField(meta.U_d, "U_d");
            RequireMetaField(meta.Motor_Speed, "Motor_Speed");
            RequireMetaField(meta.Profile_Id, "Profile_Id");
            RequireMetaField(meta.Ambient, "Ambient");
            RequireMetaField(meta.Torque, "Torque");

            if (meta.ExpectedRows <= 0)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault("ExpectedRows mora biti veci od 0.", "ExpectedRows", meta.ExpectedRows.ToString(CultureInfo.InvariantCulture)),
                    "ExpectedRows mora biti veci od 0.");
            }
        }

        private void RequireMetaField(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Meta-zaglavlje mora imati polje " + fieldName + ".", fieldName, value),
                    "Meta-zaglavlje nije ispravno.");
            }
        }

        private void EnsureSessionStarted()
        {
            if (!sessionStarted || storage == null)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Sesija nije pokrenuta. Prvo pozovi StartSession.", "Session", "not started"),
                    "Sesija nije pokrenuta.");
            }
        }

        private void SubscribeDefaultHandlers()
        {
            OnTransferStarted += meta => Console.WriteLine("prenos u toku...");
            OnSampleReceived += (sampleNumber, sample) => Console.WriteLine("ACK sample #" + sampleNumber + ": " + sample);
            OnTransferCompleted += count => Console.WriteLine("zavrsen prenos. Broj prihvacenih uzoraka: " + count);
            OnWarningRaised += (warningType, direction, message) => Console.WriteLine("DOGADJAJ " + warningType + " | " + direction + " | " + message);
        }

        private void DisposeStorageIfExists()
        {
            if (storage != null)
            {
                storage.Dispose();
                storage = null;
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(PmsmService));
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            DisposeStorageIfExists();
        }
    }
}