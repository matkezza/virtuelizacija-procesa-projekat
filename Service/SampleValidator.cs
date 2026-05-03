using System;
using System.Globalization;
using Common;

namespace ServiceHostApp
{
    public class SampleValidator
    {
        private readonly ConfigurationReader configuration;

        public SampleValidator(ConfigurationReader configuration)
        {
            this.configuration = configuration;
        }

        public string GetValidationError(MotorSample sample, double? currentSpeedAverage)
        {
            if (sample == null)
            {
                return "MotorSample nije poslat.";
            }

            if (sample.Motor_Speed <= 0)
            {
                return "Motor_Speed mora biti veci od 0.";
            }

            if (Math.Abs(sample.U_d) > configuration.UdThreshold)
            {
                return "U_d prelazi Ud_threshold. Vrednost: " + sample.U_d.ToString(CultureInfo.InvariantCulture);
            }

            if (Math.Abs(sample.U_q) > configuration.UqThreshold)
            {
                return "U_q prelazi Uq_threshold. Vrednost: " + sample.U_q.ToString(CultureInfo.InvariantCulture);
            }

            if (configuration.SpeedThreshold > 0 && Math.Abs(sample.Motor_Speed) > configuration.SpeedThreshold)
            {
                return "Motor_Speed prelazi Speed_threshold. Vrednost: " + sample.Motor_Speed.ToString(CultureInfo.InvariantCulture);
            }

            if (currentSpeedAverage.HasValue && currentSpeedAverage.Value > 0)
            {
                double lowerLimit = currentSpeedAverage.Value * (1 - configuration.AllowedDeviation);
                double upperLimit = currentSpeedAverage.Value * (1 + configuration.AllowedDeviation);

                if (sample.Motor_Speed < lowerLimit || sample.Motor_Speed > upperLimit)
                {
                    return string.Format(CultureInfo.InvariantCulture,
                        "Motor_Speed odstupa vise od ±{0:P0} od tekuceg proseka. Speed={1}, Average={2}",
                        configuration.AllowedDeviation,
                        sample.Motor_Speed,
                        currentSpeedAverage.Value);
                }
            }

            return null;
        }
    }
}
