using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Game.Persistence
{
    public sealed class MatchParticipantRepository : IMatchParticipantRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public MatchParticipantRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public Task<List<MatchParticipant>> ListByMatchesAsync(IReadOnlyCollection<int> matchIds)
        {
            Task<List<MatchParticipant>> participantsReading = ReadByMatchesAsync(matchIds);

            return participantsReading;
        }

        public Task AddAsync(int matchId, IReadOnlyList<MatchParticipant> seatedPlayers)
        {
            Task adding = AddSeatedPlayersAsync(matchId, seatedPlayers);

            return adding;
        }

        public Task WriteResultsAsync(int matchId, IReadOnlyList<MatchParticipant> playerResults)
        {
            Task resultWriting = WritePlayerResultsAsync(matchId, playerResults);

            return resultWriting;
        }

        private async Task AddSeatedPlayersAsync(int matchId, IReadOnlyList<MatchParticipant> seatedPlayers)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            foreach (MatchParticipant seatedPlayer in seatedPlayers)
            {
                databaseContext.MatchParticipants.Add(new MatchParticipant
                {
                    MatchId = matchId,
                    SeatNumber = seatedPlayer.SeatNumber,
                    PlayerId = seatedPlayer.PlayerId,
                });
            }

            await databaseContext.SaveChangesAsync();
        }

        private async Task WritePlayerResultsAsync(int matchId, IReadOnlyList<MatchParticipant> playerResults)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
            await using IDbContextTransaction resultWriting = await databaseContext.Database.BeginTransactionAsync();

            foreach (MatchParticipant playerResult in playerResults)
            {
                await databaseContext.MatchParticipants
                    .Where(storedParticipant => (storedParticipant.MatchId == matchId) && (storedParticipant.SeatNumber == playerResult.SeatNumber))
                    .ExecuteUpdateAsync(participantUpdate => participantUpdate
                        .SetProperty(storedParticipant => storedParticipant.FinalScore, playerResult.FinalScore)
                        .SetProperty(storedParticipant => storedParticipant.FinalPosition, playerResult.FinalPosition)
                        .SetProperty(storedParticipant => storedParticipant.WasWithdrawn, playerResult.WasWithdrawn));
            }

            await resultWriting.CommitAsync();
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
