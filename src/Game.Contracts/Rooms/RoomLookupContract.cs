using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class RoomLookupContract
    {
        [DataMember]
        public RoomLookupStatus Status { get; set; }

        [DataMember]
        public RoomContract? Room { get; set; }
    }
}
