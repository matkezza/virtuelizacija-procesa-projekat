using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public enum SessionStatus
    {
        [EnumMember]
        IN_PROGRESS = 0,

        [EnumMember]
        COMPLETED = 1
    }
}
