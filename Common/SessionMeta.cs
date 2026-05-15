using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class SessionMeta
    {
        [DataMember]
        public string U_q { get; set; }

        [DataMember]
        public string U_d { get; set; }

        [DataMember]
        public string Motor_Speed { get; set; }

        [DataMember]
        public string Profile_Id { get; set; }

        [DataMember]
        public string Ambient { get; set; }

        [DataMember]
        public string Torque { get; set; }

        [DataMember]
        public string SourceFileName { get; set; }

        [DataMember]
        public int ExpectedRows { get; set; }

        public SessionMeta()
        {
        }

        public SessionMeta(string sourceFileName, int expectedRows)
        {
            U_q = "u_q";
            U_d = "u_d";
            Motor_Speed = "motor_speed";
            Profile_Id = "profile_id";
            Ambient = "ambient";
            Torque = "torque";
            SourceFileName = sourceFileName;
            ExpectedRows = expectedRows;
        }
    }
}