using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CoreWCF;

using Game.Contracts;
using Game.Services.Persistence;

using log4net;

using Npgsql;

namespace Game.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public sealed class RankingService : IRankingService
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(RankingService));

        private readonly RankingRepository _rankingRepository;

        public RankingService(RankingRepository rankingRepository)
        {
            ArgumentNullException.ThrowIfNull(rankingRepository);
            _rankingRepository = rankingRepository;
        }

        public async Task<GlobalRankingResponseContract> GetGlobalRankingAsync(int currentUserId)
        {
            IReadOnlyList<RankingRow> rows;
            try
            {
                rows = await _rankingRepository.GetGlobalRankingAsync(currentUserId);
            }
            catch (NpgsqlException exception)
            {
                _log.Error("Error getting global ranking.", exception);
                throw;
            }

            List<RankingEntryContract> entries = rows
                .Select(row => new RankingEntryContract
                {
                    Rank = (int)row.Rank,
                    PlayerName = row.Username,
                    Wins = (int)row.Wins,
                    Points = (int)row.Points,
                    Matches = (int)row.Matches,
                    IsCurrentPlayer = row.Id == currentUserId,
                }).ToList();

            return new GlobalRankingResponseContract { Entries = entries };
        }
    }
}
