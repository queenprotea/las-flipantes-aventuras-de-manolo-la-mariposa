using CoreWCF;

using Game.Contracts;

namespace Game.Services
{
    /// <summary>
    /// Reads the caller from the operation context of the current CoreWCF call.
    /// </summary>
    public sealed class CallerChannel : ICallerChannel
    {
        public string? FindCallerSessionId()
        {
            string? callerSessionId = OperationContext.Current?.SessionId;

            return callerSessionId;
        }

        public IGameSessionNoticeReceiver GetNoticeReceiver()
        {
            IGameSessionNoticeReceiver noticeReceiver = OperationContext.Current.GetCallbackChannel<IGameSessionNoticeReceiver>();

            return noticeReceiver;
        }
    }
}
