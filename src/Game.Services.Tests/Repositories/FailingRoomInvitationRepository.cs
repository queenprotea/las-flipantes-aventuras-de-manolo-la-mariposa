using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Persistence;

using Npgsql;

namespace Game.Services.Tests
{
    internal sealed class FailingRoomInvitationRepository : IRoomInvitationRepository
    {
        private const string FailureMessage = "The test database is down.";

        public Task<InvitationCreationOutcome> CreateAsync(RoomInvitationRecord newInvitation)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<List<RoomInvitationRecord>> ListForInviteeAsync(int invitedPlayerId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<List<int>> ListInviteeIdsAsync(int roomId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<bool> ExistsAsync(int roomId, int invitedPlayerId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<string?> FindChannelAsync(int roomId, int invitedPlayerId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task ChangeChannelAsync(int roomId, int invitedPlayerId, string channel)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<bool> DeleteAsync(int roomId, int invitedPlayerId)
        {
            throw new NpgsqlException(FailureMessage);
        }
    }
}
