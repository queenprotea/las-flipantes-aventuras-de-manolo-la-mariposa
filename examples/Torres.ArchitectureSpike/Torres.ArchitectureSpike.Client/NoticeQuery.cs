using Torres.ArchitectureSpike.Contracts;

namespace Torres.ArchitectureSpike.Client
{
    internal sealed class NoticeQuery
    {
        private readonly MatchEventCode? _eventCode;
        private readonly string? _playerName;
        private readonly ConnectionExpectation _connectionExpectation;

        private NoticeQuery(MatchEventCode? eventCode, string? playerName, ConnectionExpectation connectionExpectation)
        {
            _eventCode = eventCode;
            _playerName = playerName;
            _connectionExpectation = connectionExpectation;
        }

        internal static NoticeQuery ForMatchEvent(MatchEventCode eventCode)
        {
            var query = new NoticeQuery(eventCode, null, ConnectionExpectation.None);

            return query;
        }

        internal static NoticeQuery ForConnectionOf(string playerName)
        {
            var query = new NoticeQuery(null, playerName, ConnectionExpectation.Connected);

            return query;
        }

        internal static NoticeQuery ForLossOf(string playerName)
        {
            var query = new NoticeQuery(null, playerName, ConnectionExpectation.Lost);

            return query;
        }

        internal bool Matches(ReceivedNotice notice)
        {
            bool isExpectedMatchEvent = (notice.MatchUpdate is not null) && (notice.MatchUpdate.EventCode == _eventCode);
            bool isExpectedConnectionChange = (notice.ConnectionNotice is not null)
                && (notice.ConnectionNotice.PlayerName == _playerName)
                && (ClassifyChange(notice.ConnectionNotice.Change) == _connectionExpectation);
            bool isMatch = isExpectedMatchEvent || isExpectedConnectionChange;

            return isMatch;
        }

        private static ConnectionExpectation ClassifyChange(PlayerConnectionChange change)
        {
            ConnectionExpectation expectation = (change == PlayerConnectionChange.Connected) ? ConnectionExpectation.Connected : ConnectionExpectation.Lost;

            return expectation;
        }
    }
}
