using System;
using System.IO;
using Common;

namespace ServiceHostApp
{
    public class SessionFileStorage : IDisposable
    {
        private TextWriter measurementsWriter;
        private TextWriter rejectsWriter;
        private TextWriter logWriter;
        private bool disposed = false;
        private readonly string sessionDirectoryPath;

        public string SessionDirectoryPath
        {
            get { return sessionDirectoryPath; }
        }

        public SessionFileStorage(string baseStoragePath)
        {
            if (string.IsNullOrWhiteSpace(baseStoragePath))
            {
                baseStoragePath = "Storage";
            }

            if (!Directory.Exists(baseStoragePath))
            {
                Directory.CreateDirectory(baseStoragePath);
            }

            string sessionName = "session_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            sessionDirectoryPath = Path.Combine(baseStoragePath, sessionName);
            Directory.CreateDirectory(sessionDirectoryPath);

            measurementsWriter = new StreamWriter(Path.Combine(sessionDirectoryPath, "measurements_session.csv"), false);
            rejectsWriter = new StreamWriter(Path.Combine(sessionDirectoryPath, "rejects.csv"), false);
            logWriter = new StreamWriter(Path.Combine(sessionDirectoryPath, "validation.log"), false);

            measurementsWriter.WriteLine("U_q,U_d,Motor_Speed,Profile_Id,Ambient,Torque");
            rejectsWriter.WriteLine("Reason,U_q,U_d,Motor_Speed,Profile_Id,Ambient,Torque");
            logWriter.WriteLine("Session created: " + DateTime.Now);
        }

        ~SessionFileStorage()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (measurementsWriter != null)
                    {
                        measurementsWriter.Dispose();
                        measurementsWriter = null;
                    }

                    if (rejectsWriter != null)
                    {
                        rejectsWriter.Dispose();
                        rejectsWriter = null;
                    }

                    if (logWriter != null)
                    {
                        logWriter.Dispose();
                        logWriter = null;
                    }
                }

                disposed = true;
            }
        }

        public void WriteAcceptedSample(MotorSample sample)
        {
            measurementsWriter.WriteLine(sample.ToCsvLine());
            measurementsWriter.Flush();
        }

        public void WriteRejectedSample(MotorSample sample, string reason)
        {
            rejectsWriter.WriteLine(Escape(reason) + "," + sample.ToCsvLine());
            rejectsWriter.Flush();
            WriteLog("REJECTED: " + reason + " | " + sample);
        }

        public void WriteLog(string message)
        {
            logWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " | " + message);
            logWriter.Flush();
        }

        private string Escape(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
