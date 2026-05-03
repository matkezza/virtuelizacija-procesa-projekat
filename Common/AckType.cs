using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public enum AckType
    {
        [EnumMember]
        ACK = 0,

        [EnumMember]
        NACK = 1
    }
}
