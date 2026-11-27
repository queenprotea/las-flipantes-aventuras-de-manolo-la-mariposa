using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Match.Tests
{
    public sealed class AbsentSeatTests
    {
        private const int OneExpiredStep = 1;
        private const int TwoExpiredSteps = 2;
        private const int OtherMatchId = 99;

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly RecordingMatchListener _listener = new RecordingMatchListener();
        private readonly MatchCoordinator _coordinator;

        public AbsentSeatTests()
        {
            var matchRepositories = new MatchRepositories(new RecordingMatchRecordRepository(), new RecordingMatchParticipantRepository(), new RecordingMatchStateRepository());
            _coordinator = new MatchCoordinator(_fakeTimeProvider, _listener, matchRepositories, SampleMatches.FiveFlowerCells, new Random(SampleMatches.ChanceSeed));
        }

        [Fact]
        public async Task Test_ExpireStep_ReachingTheAbsentLimit_ReportsTheSeatAbsent()
        {
            await StartAnaAndBetoAsync(SampleMatches.TimeLimits with { AbsentAfterExpiredSteps = OneExpiredStep });

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            Assert.Equal(new int[] { SampleMatches.Ana.EntrantId }, _listener.AbsentEntrantIds);
        }

        [Fact]
        public async Task Test_ExpireStep_OfDifferentSeatsBelowTheLimit_ReportsNobody()
        {
            await StartAnaAndBetoAsync(SampleMatches.TimeLimits with { AbsentAfterExpiredSteps = TwoExpiredSteps });
            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            Assert.Empty(_listener.AbsentEntrantIds);
        }

        [Fact]
        public async Task Test_ExpireStep_WithoutAbsentLimit_ReportsNobody()
        {
            await StartAnaAndBetoAsync(SampleMatches.TimeLimits);

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            Assert.Empty(_listener.AbsentEntrantIds);
        }

        [Fact]
        public async Task Test_HoldsSeatIn_OwnMatch_ReturnsTrue()
        {
            int matchId = await StartAnaAndBetoAsync(SampleMatches.TimeLimits);

            bool holdsSeat = _coordinator.HoldsSeatIn(SampleMatches.Ana.EntrantId, matchId);

            Assert.True(holdsSeat);
        }

        [Fact]
        public async Task Test_HoldsSeatIn_AnotherMatch_ReturnsFalse()
        {
            await StartAnaAndBetoAsync(SampleMatches.TimeLimits);

            bool holdsSeat = _coordinator.HoldsSeatIn(SampleMatches.Ana.EntrantId, OtherMatchId);

            Assert.False(holdsSeat);
        }

        [Fact]
        public async Task Test_HoldsSeatIn_AfterForfeiting_ReturnsFalse()
        {
            int matchId = await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaBetoAndDani), SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.Forfeit(SampleMatches.Dani.EntrantId);

            bool holdsSeat = _coordinator.HoldsSeatIn(SampleMatches.Dani.EntrantId, matchId);

            Assert.False(holdsSeat);
        }

        private Task<int> StartAnaAndBetoAsync(MatchTimeLimits timeLimits)
        {
            Task<int> matchStart = _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), timeLimits, SampleMatches.Rules);

            return matchStart;
        }
    }
}
