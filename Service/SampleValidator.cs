using System;
using Common;

namespace ServiceHostApp
{
    public class SampleValidator
    {
        public string GetValidationError(MotorSample sample)
        {
            if (sample == null)
            {
                return "MotorSample nije poslat.";
            }

            if (!IsFinite(sample.U_q))
            {
                return "U_q mora biti validan broj.";
            }

            if (!IsFinite(sample.U_d))
            {
                return "U_d mora biti validan broj.";
            }

            if (!IsFinite(sample.Motor_Speed))
            {
                return "Motor_Speed mora biti validan broj.";
            }

            if (!IsFinite(sample.Ambient))
            {
                return "Ambient mora biti validan broj.";
            }

            if (!IsFinite(sample.Torque))
            {
                return "Torque mora biti validan broj.";
            }

            if (sample.Motor_Speed <= 0)
            {
                return "Motor_Speed mora biti veci od 0.";
            }

            if (sample.Profile_Id < 0)
            {
                return "Profile_Id ne sme biti negativan.";
            }

            return null;
        }

        private bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}