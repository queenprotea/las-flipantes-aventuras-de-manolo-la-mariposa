using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class CreateRoomResponseContract
    {
        [DataMember]
        public CreateRoomStatus Status { get; set; }

        [DataMember]
        public RoomContract? Room { get; set; }
    }
}
