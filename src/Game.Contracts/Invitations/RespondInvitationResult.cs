using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the answer to a room invitation and, when it was accepted, the room entered.
    /// </summary>
    [DataContract]
    public sealed class RespondInvitationResult
    {
        [DataMember]
        public RespondInvitationStatus Status { get; set; }

        [DataMember]
        public Room? Room { get; set; }
    }
}
