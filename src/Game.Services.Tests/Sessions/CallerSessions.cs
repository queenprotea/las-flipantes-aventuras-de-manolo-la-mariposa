using System.Globalization;

namespace Game.Services.Tests
{
    internal sealed class CallerSessions
    {
        private const string SessionIdFormat = "session-{0}";

        private readonly RecordingCallerChannel _callerChannel = new RecordingCallerChannel();

        internal CallerSessions()
        {
            Registry = new SessionRegistry(_callerChannel);
        }

        internal SessionRegistry Registry { get; }

        internal void Open(SessionOwner owner)
        {
            ActAs(owner);
            Registry.Open(owner);
        }

        internal void ActAs(SessionOwner owner)
        {
            _callerChannel.CallerSessionId = FindSessionId(owner);
        }

        internal void Lose(SessionOwner owner)
        {
            Registry.DeclareAbsent(owner.OwnerId);
        }

        internal RecordingNoticeReceiver FindReceiverOf(SessionOwner owner)
        {
            RecordingNoticeReceiver noticeReceiver = _callerChannel.FindReceiver(FindSessionId(owner));

            return noticeReceiver;
        }

        private static string FindSessionId(SessionOwner owner)
        {
            string sessionId = string.Format(CultureInfo.InvariantCulture, SessionIdFormat, owner.OwnerId);

            return sessionId;
        }
    }
}
