using Game.Contracts;

namespace Game.Services
{
    internal abstract class Notice
    {
        internal abstract void SendTo(IGameSessionNoticeReceiver noticeReceiver);
    }
}
