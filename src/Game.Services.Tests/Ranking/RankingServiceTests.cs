using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Persistence;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class RankingServiceTests
    {
        private const int AnaWins = 3;
        private const int BetoWins = 1;

        private readonly RecordingRankingRepository _rankingRepository = new RecordingRankingRepository();
        private readonly RankingService _rankingService;

        public RankingServiceTests()
        {
            _rankingService = new RankingService(_rankingRepository);
        }

        [Fact]
        public async Task Test_GetGlobalRankingAsync_WithStoredRankings_ReturnsThemInTheStoredOrder()
        {
            _rankingRepository.Rankings.Add(new PlayerRanking { PlayerId = SamplePlayers.AnaId, Username = SamplePlayers.AnaUsername, Wins = AnaWins });
            _rankingRepository.Rankings.Add(new PlayerRanking { PlayerId = SamplePlayers.BetoId, Username = SamplePlayers.BetoUsername, Wins = BetoWins });

            GlobalRankingResponse ranking = await _rankingService.GetGlobalRankingAsync();

            Assert.Equal(new string[] { SamplePlayers.AnaUsername, SamplePlayers.BetoUsername }, ranking.Entries.Select(entry => entry.PlayerName));
        }

        [Fact]
        public async Task Test_GetGlobalRankingAsync_WithStoredRankings_ReturnsTheWins()
        {
            _rankingRepository.Rankings.Add(new PlayerRanking { PlayerId = SamplePlayers.AnaId, Username = SamplePlayers.AnaUsername, Wins = AnaWins });

            GlobalRankingResponse ranking = await _rankingService.GetGlobalRankingAsync();

            Assert.Equal(new int[] { AnaWins }, ranking.Entries.Select(entry => entry.Wins));
        }

        [Fact]
        public async Task Test_GetGlobalRankingAsync_WithoutFinishedMatches_ReturnsNoEntries()
        {
            GlobalRankingResponse ranking = await _rankingService.GetGlobalRankingAsync();

            Assert.Empty(ranking.Entries);
        }
    }
}
