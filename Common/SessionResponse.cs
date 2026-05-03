using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class SessionResponse
    {
        [DataMember]
        public AckType AckType { get; set; }

        [DataMember]
        public SessionStatus Status { get; set; }

        [DataMember]
        public string Message { get; set; }

        public SessionResponse()
        {
        }

        public SessionResponse(AckType ackType, SessionStatus status, string message)
        {
            AckType = ackType;
            Status = status;
            Message = message;
        }

        public override string ToString()
        {
            return AckType + " | " + Status + " | " + Message;
        }
    }
}
