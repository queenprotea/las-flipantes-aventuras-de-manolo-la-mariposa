using Game.Contracts;

namespace Game.Services
{
    internal sealed class RoomInvitationNotice : Notice
    {
        private readonly RoomInvitation _invitation;

        internal RoomInvitationNotice(RoomInvitation invitation)
        {
            _invitation = invitation;
        }

        internal override void SendTo(IGameSessionNoticeReceiver noticeReceiver)
        {
            noticeReceiver.ReceiveRoomInvitation(_invitation);
        }
    }
}
