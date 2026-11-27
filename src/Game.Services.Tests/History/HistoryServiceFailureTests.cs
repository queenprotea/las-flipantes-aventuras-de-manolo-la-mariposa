using System.Threading.Tasks;

using CoreWCF;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class HistoryServiceFailureTests
    {
        private const int MatchesPerPage = 10;
        private const int FirstPage = 1;
        private const int SomeMatchId = 1;

        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly HistoryService _historyService;

        public HistoryServiceFailureTests()
        {
            _historyService = new HistoryService(_callerSessions.Registry, new FailingMatchRecordRepository(), new RecordingMatchParticipantRepository(), new RecordingPlayerRepository(), new HistoryPaging(MatchesPerPage));
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public async Task Test_GetMatchHistoryAsync_WithTheDatabaseDown_ThrowsFaultException()
        {
            await Assert.ThrowsAsync<FaultException>(() => _historyService.GetMatchHistoryAsync(FirstPage));
        }

        [Fact]
        public async Task Test_GetMatchDetailAsync_WithTheDatabaseDown_ThrowsFaultException()
        {
            await Assert.ThrowsAsync<FaultException>(() => _historyService.GetMatchDetailAsync(SomeMatchId));
        }
    }
}
