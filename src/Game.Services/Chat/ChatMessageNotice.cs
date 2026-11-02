using Game.Contracts;

namespace Game.Services
{
    internal sealed class ChatMessageNotice : Notice
    {
        private readonly ChatMessage _message;

        internal ChatMessageNotice(ChatMessage message)
        {
            _message = message;
        }

        internal override void SendTo(IGameSessionNoticeReceiver noticeReceiver)
        {
            noticeReceiver.ReceiveChatMessage(_message);
        }
    }
}
