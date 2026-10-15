using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class RespondInvitationResponseContract
    {
        [DataMember]
        public RespondInvitationResult Result { get; set; }

        [DataMember]
        public RoomContract? Room { get; set; }
    }
}
