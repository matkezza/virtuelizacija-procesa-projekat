using System.Globalization;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class MotorSample
    {
        [DataMember]
        public double U_q { get; set; }

        [DataMember]
        public double U_d { get; set; }

        [DataMember]
        public double Motor_Speed { get; set; }

        [DataMember]
        public int Profile_Id { get; set; }

        [DataMember]
        public double Ambient { get; set; }

        [DataMember]
        public double Torque { get; set; }

        public MotorSample()
        {
        }

        public MotorSample(double uQ, double uD, double motorSpeed, int profileId, double ambient, double torque)
        {
            U_q = uQ;
            U_d = uD;
            Motor_Speed = motorSpeed;
            Profile_Id = profileId;
            Ambient = ambient;
            Torque = torque;
        }

        public string ToCsvLine()
        {
            return string.Join(",",
                U_q.ToString(CultureInfo.InvariantCulture),
                U_d.ToString(CultureInfo.InvariantCulture),
                Motor_Speed.ToString(CultureInfo.InvariantCulture),
                Profile_Id.ToString(CultureInfo.InvariantCulture),
                Ambient.ToString(CultureInfo.InvariantCulture),
                Torque.ToString(CultureInfo.InvariantCulture));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "U_q={0}, U_d={1}, Motor_Speed={2}, Profile_Id={3}, Ambient={4}, Torque={5}",
                U_q, U_d, Motor_Speed, Profile_Id, Ambient, Torque);
        }
    }
}
