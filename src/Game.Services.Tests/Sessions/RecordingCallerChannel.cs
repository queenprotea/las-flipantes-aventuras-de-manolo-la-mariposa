using System.Collections.Generic;
using System.Threading;

using Game.Contracts;

namespace Game.Services.Tests
{
    internal sealed class RecordingCallerChannel : ICallerChannel
    {
        private readonly Lock _channelLock = new Lock();
        private readonly Dictionary<string, RecordingNoticeReceiver> _receiversBySessionId = new Dictionary<string, RecordingNoticeReceiver>();

        internal string? CallerSessionId { get; set; }

        public string? FindCallerSessionId()
        {
            string? callerSessionId = CallerSessionId;

            return callerSessionId;
        }

        public IGameSessionNoticeReceiver GetNoticeReceiver()
        {
            RecordingNoticeReceiver noticeReceiver = FindReceiver(CallerSessionId ?? string.Empty);

            return noticeReceiver;
        }

        internal RecordingNoticeReceiver FindReceiver(string sessionId)
        {
            RecordingNoticeReceiver? noticeReceiver;
            lock (_channelLock)
            {
                if (!_receiversBySessionId.TryGetValue(sessionId, out noticeReceiver))
                {
                    noticeReceiver = new RecordingNoticeReceiver();
                    _receiversBySessionId.Add(sessionId, noticeReceiver);
                }
            }

            return noticeReceiver;
        }
    }
}
