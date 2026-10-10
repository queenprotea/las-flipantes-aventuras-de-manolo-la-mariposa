using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CoreWCF;

using Game.Contracts;
using Game.Persistence;

using log4net;

using Npgsql;

namespace Game.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public sealed class RankingService : IRankingService
    {
        private const string RankingUnavailableReason = "The global ranking is not available right now.";
        private const string RankingReadFailedMessage = "The database could not be read while building the global ranking.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(RankingService));

        private readonly RankingRepository _rankingRepository;

        public RankingService(RankingRepository rankingRepository)
        {
            ArgumentNullException.ThrowIfNull(rankingRepository);

            _rankingRepository = rankingRepository;
        }

        public async Task<GlobalRankingResponseContract> GetGlobalRankingAsync()
        {
            List<PlayerRanking> rankings;
            try
            {
                rankings = await _rankingRepository.GetGlobalRankingAsync();
            }
            catch (NpgsqlException exception)
            {
                _log.Error(RankingReadFailedMessage, exception);
                throw new FaultException(RankingUnavailableReason);
            }

            List<RankingEntryContract> entries = rankings.Select(ToContract).ToList();
            var response = new GlobalRankingResponseContract { Entries = entries };

            return response;
        }

        private static RankingEntryContract ToContract(PlayerRanking ranking)
        {
            var entry = new RankingEntryContract
            {
                PlayerName = ranking.Username,
                Wins = (int)ranking.Wins,
                Points = (int)ranking.TotalScore,
                Matches = (int)ranking.MatchesPlayed,
            };

            return entry;
        }
    }
}
