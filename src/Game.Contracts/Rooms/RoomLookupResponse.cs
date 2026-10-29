using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class RoomLookupResponse
    {
        [DataMember]
        public RoomLookupStatus Status { get; set; }

        [DataMember]
        public Room? Room { get; set; }
    }
}
