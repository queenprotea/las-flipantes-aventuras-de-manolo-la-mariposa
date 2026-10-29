using Game.Contracts;

namespace Game.Services
{
    internal sealed class RoomChangeNotice : Notice
    {
        private readonly RoomChangeReason _reason;
        private readonly Room _room;

        internal RoomChangeNotice(RoomChangeReason reason, Room room)
        {
            _reason = reason;
            _room = room;
        }

        internal override void SendTo(IGameSessionNoticeReceiver noticeReceiver)
        {
            noticeReceiver.ReceiveRoomStateChanged(_reason, _room);
        }
    }
}
