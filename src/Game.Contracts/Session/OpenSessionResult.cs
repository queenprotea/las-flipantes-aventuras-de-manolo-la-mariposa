using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class OpenSessionResult
    {
        [DataMember]
        public OpenSessionStatus Status { get; set; }

        [DataMember]
        public SessionIdentity? Identity { get; set; }

        [DataMember]
        public Room? CurrentRoom { get; set; }

        [DataMember]
        public int ConnectionCheckPeriodSeconds { get; set; }
    }
}
