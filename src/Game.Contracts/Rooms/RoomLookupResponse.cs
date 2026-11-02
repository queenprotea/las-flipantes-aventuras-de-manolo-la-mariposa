using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the room the caller is in, when there is one.
    /// </summary>
    [DataContract]
    public sealed class RoomLookupResponse
    {
        [DataMember]
        public RoomLookupStatus Status { get; set; }

        [DataMember]
        public Room? Room { get; set; }
    }
}
