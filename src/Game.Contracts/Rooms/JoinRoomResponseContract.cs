using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class JoinRoomResponseContract
    {
        [DataMember]
        public JoinRoomStatus Status { get; set; }

        [DataMember]
        public RoomContract? Room { get; set; }
    }
}
