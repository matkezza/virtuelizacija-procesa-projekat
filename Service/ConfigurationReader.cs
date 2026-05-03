using System;
using System.Configuration;
using System.Globalization;

namespace ServiceHostApp
{
    public class ConfigurationReader
    {
        public double UdThreshold { get; private set; }
        public double UqThreshold { get; private set; }
        public double SpeedThreshold { get; private set; }
        public double AllowedDeviation { get; private set; }
        public string StoragePath { get; private set; }

        public ConfigurationReader()
        {
            UdThreshold = ReadDouble("Ud_threshold", 1000);
            UqThreshold = ReadDouble("Uq_threshold", 1000);
            SpeedThreshold = ReadDouble("Speed_threshold", 10000);
            AllowedDeviation = ReadDouble("AllowedDeviation", 0.25);
            StoragePath = ReadString("storagePath", "Storage");
        }

        private double ReadDouble(string key, double defaultValue)
        {
            string raw = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrWhiteSpace(raw))
            {
                return defaultValue;
            }

            double value;
            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return defaultValue;
            }

            return value;
        }

        private string ReadString(string key, string defaultValue)
        {
            string raw = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(raw) ? defaultValue : raw;
        }
    }
}
