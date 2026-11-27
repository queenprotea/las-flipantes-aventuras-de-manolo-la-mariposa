using Game.Contracts;

namespace Game.Services
{
    /// <summary>
    /// Tells which session is behind the current call and gives the channel that sends notices back to it.
    /// </summary>
    public interface ICallerChannel
    {
        string? FindCallerSessionId();

        IGameSessionNoticeReceiver GetNoticeReceiver();
    }
}
