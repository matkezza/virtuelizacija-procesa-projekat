using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class SessionMeta
    {
        [DataMember]
        public string[] Headers { get; set; }

        [DataMember]
        public string SourceFileName { get; set; }

        [DataMember]
        public int ExpectedRows { get; set; }

        public SessionMeta()
        {
        }

        public SessionMeta(string sourceFileName, int expectedRows)
        {
            SourceFileName = sourceFileName;
            ExpectedRows = expectedRows;
            Headers = new string[]
            {
                "U_q",
                "U_d",
                "Motor_Speed",
                "Profile_Id",
                "Ambient",
                "Torque"
            };
        }
    }
}
