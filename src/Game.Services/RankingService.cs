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
        private static ILog _log = LogManager.GetLogger(typeof(RankingService));

        private readonly RankingRepository _rankingRepository;

        public RankingService(RankingRepository rankingRepository)
        {
            ArgumentNullException.ThrowIfNull(rankingRepository);
            _rankingRepository = rankingRepository;
        }

        public async Task<GlobalRankingResponseContract> GetGlobalRankingAsync()
        {
            List<RankingRow> rows;
            try
            {
                rows = await _rankingRepository.GetGlobalRankingAsync();
            }
            catch (NpgsqlException exception)
            {
                _log.Error("Error getting global ranking.", exception);
                throw;
            }

            var entries = rows
                .Select(row => new RankingEntryContract
                {
                    PlayerName = row.Username,
                    Wins = (int)row.Wins,
                    Points = (int)row.TotalScore,
                    Matches = (int)row.MatchesPlayed,
                })
                .ToList();

            return new GlobalRankingResponseContract { Entries = entries };
        }
    }
}
