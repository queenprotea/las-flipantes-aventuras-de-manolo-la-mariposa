using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class RespondInvitationResult
    {
        [DataMember]
        public RespondInvitationStatus Status { get; set; }

        [DataMember]
        public Room? Room { get; set; }
    }
}
