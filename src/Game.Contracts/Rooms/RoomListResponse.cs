using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class RoomListResponse
    {
        [DataMember]
        public List<RoomSummary> Rooms { get; set; } = new List<RoomSummary>();
    }
}
