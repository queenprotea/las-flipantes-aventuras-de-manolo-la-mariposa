using System.Collections.Generic;

using Game.Persistence;

using Npgsql;

namespace Game.Services.Tests
{
    internal sealed class FailingFriendshipRepository : IFriendshipRepository
    {
        private const string FailureMessage = "The test database is down.";

        public List<Friendship> ListByPlayer(int playerId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Friendship? FindBetween(int firstPlayerId, int secondPlayerId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public FriendRequestCreationOutcome CreateRequest(int requesterId, int addresseeId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public bool AcceptRequest(int requesterId, int addresseeId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public bool DeleteRequest(int requesterId, int addresseeId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public bool DeleteFriendship(int firstPlayerId, int secondPlayerId)
        {
            throw new NpgsqlException(FailureMessage);
        }
    }
}
