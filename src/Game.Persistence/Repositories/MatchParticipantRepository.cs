using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Game.Persistence
{
    public sealed class MatchParticipantRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public MatchParticipantRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);

            _contextFactory = contextFactory;
        }

        public Task<List<MatchParticipant>> ListByMatchesAsync(IReadOnlyCollection<int> matchIds)
        {
            ArgumentNullException.ThrowIfNull(matchIds);

            Task<List<MatchParticipant>> participantsReading = ReadByMatchesAsync(matchIds);

            return participantsReading;
        }

        private async Task<List<MatchParticipant>> ReadByMatchesAsync(IReadOnlyCollection<int> matchIds)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            List<MatchParticipant> participants = await databaseContext.MatchParticipants
                .AsNoTracking()
                .Where(storedParticipant => matchIds.Contains(storedParticipant.MatchId))
                .ToListAsync();

            return participants;
        }
    }
}
