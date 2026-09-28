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

        private static readonly ILog _log = LogManager.GetLogger(typeof(RankingService));

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
                _log.Error("The database could not be read while building the global ranking.", exception);
                throw new FaultException(RankingUnavailableReason);
            }

            List<RankingEntryContract> entries = rows.Select(ToContract).ToList();

            return new GlobalRankingResponseContract { Entries = entries };
        }

        private static RankingEntryContract ToContract(RankingRow row)
        {
            return new RankingEntryContract
            {
                PlayerName = row.Username,
                Wins = (int)row.Wins,
                Points = (int)row.TotalScore,
                Matches = (int)row.MatchesPlayed,
            };
        }
    }
}
