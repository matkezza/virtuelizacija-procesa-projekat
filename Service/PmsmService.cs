using System;
using System.ServiceModel;
using Common;

namespace ServiceHostApp
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PmsmService : IPmsmService
    {
        private readonly ConfigurationReader configuration;
        private readonly SampleValidator validator;
        private SessionFileStorage storage;
        private bool sessionStarted = false;
        private int acceptedSamplesCount = 0;
        private double speedSum = 0;

        public PmsmService()
        {
            configuration = new ConfigurationReader();
            validator = new SampleValidator(configuration);
        }

        public SessionResponse StartSession(SessionMeta meta)
        {
            if (meta == null || meta.Headers == null || meta.Headers.Length == 0)
            {
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Meta-zaglavlje nije ispravno.", "Headers", 0),
                    "Meta-zaglavlje nije ispravno.");
            }

            DisposeStorageIfExists();

            storage = new SessionFileStorage(configuration.StoragePath);
            sessionStarted = true;
            acceptedSamplesCount = 0;
            speedSum = 0;

            storage.WriteLog("StartSession. Source=" + meta.SourceFileName + ", ExpectedRows=" + meta.ExpectedRows);
            Console.WriteLine("StartSession primljen. Skladiste: " + storage.SessionDirectoryPath);

            return new SessionResponse(AckType.ACK, SessionStatus.IN_PROGRESS, "Sesija je pokrenuta.");
        }

        public SessionResponse PushSample(MotorSample sample)
        {
            if (!sessionStarted)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Sesija nije pokrenuta. Prvo pozovi StartSession.", "Session", "not started"),
                    "Sesija nije pokrenuta.");
            }

            if (sample == null)
            {
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Sample je null.", "MotorSample", 0),
                    "Sample je null.");
            }

            double? currentAverage = acceptedSamplesCount > 0 ? (double?)(speedSum / acceptedSamplesCount) : null;
            string validationError = validator.GetValidationError(sample, currentAverage);

            if (validationError != null)
            {
                storage.WriteRejectedSample(sample, validationError);
                Console.WriteLine("NACK: " + validationError);
                return new SessionResponse(AckType.NACK, SessionStatus.IN_PROGRESS, validationError);
            }

            storage.WriteAcceptedSample(sample);
            acceptedSamplesCount++;
            speedSum += sample.Motor_Speed;

            Console.WriteLine("ACK: " + sample);
            return new SessionResponse(AckType.ACK, SessionStatus.IN_PROGRESS, "Sample prihvacen.");
        }

        public SessionResponse EndSession()
        {
            if (!sessionStarted)
            {
                return new SessionResponse(AckType.NACK, SessionStatus.COMPLETED, "Sesija nije bila pokrenuta.");
            }

            storage.WriteLog("EndSession. AcceptedSamples=" + acceptedSamplesCount);
            DisposeStorageIfExists();
            sessionStarted = false;

            Console.WriteLine("EndSession primljen. Broj prihvacenih uzoraka: " + acceptedSamplesCount);
            return new SessionResponse(AckType.ACK, SessionStatus.COMPLETED, "Sesija je zavrsena.");
        }

        private void DisposeStorageIfExists()
        {
            if (storage != null)
            {
                storage.Dispose();
                storage = null;
            }
        }
    }
}
