using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class RoomListContract
    {
        [DataMember]
        public List<RoomSummaryContract> Rooms { get; set; } = new List<RoomSummaryContract>();
    }
}
