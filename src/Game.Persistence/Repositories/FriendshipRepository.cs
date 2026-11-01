using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Game.Persistence
{
    public sealed class FriendshipRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public FriendshipRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);

            _contextFactory = contextFactory;
        }

        public List<Friendship> ListByPlayer(int playerId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(playerId);

            using TorresDbContext databaseContext = _contextFactory.CreateDbContext();

            List<Friendship> friendships = databaseContext.Friendships
                .AsNoTracking()
                .Where(storedFriendship => (storedFriendship.RequesterId == playerId) || (storedFriendship.AddresseeId == playerId))
                .ToList();

            return friendships;
        }

        public Friendship? FindBetween(int firstPlayerId, int secondPlayerId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(firstPlayerId);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(secondPlayerId);

            using TorresDbContext databaseContext = _contextFactory.CreateDbContext();

            Friendship? friendship = databaseContext.Friendships
                .AsNoTracking()
                .Where(storedFriendship => (storedFriendship.RequesterId == firstPlayerId) || (storedFriendship.RequesterId == secondPlayerId))
                .Where(storedFriendship => (storedFriendship.AddresseeId == firstPlayerId) || (storedFriendship.AddresseeId == secondPlayerId))
                .SingleOrDefault();

            return friendship;
        }

        public FriendRequestCreationOutcome CreateRequest(int requesterId, int addresseeId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requesterId);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(addresseeId);

            using TorresDbContext databaseContext = _contextFactory.CreateDbContext();
            var newRequest = new Friendship
            {
                RequesterId = requesterId,
                AddresseeId = addresseeId,
                Status = FriendshipStatuses.Pending,
            };
            databaseContext.Friendships.Add(newRequest);
            FriendRequestCreationOutcome creationOutcome = FriendRequestCreationOutcome.Created;
            try
            {
                databaseContext.SaveChanges();
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                creationOutcome = FriendRequestCreationOutcome.PairTaken;
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
            {
                creationOutcome = FriendRequestCreationOutcome.PlayerMissing;
            }

            return creationOutcome;
        }

        public bool AcceptRequest(int requesterId, int addresseeId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requesterId);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(addresseeId);

            using TorresDbContext databaseContext = _contextFactory.CreateDbContext();
            Friendship? pendingRequest = databaseContext.Friendships
                .Where(storedFriendship => (storedFriendship.RequesterId == requesterId) && (storedFriendship.AddresseeId == addresseeId))
                .SingleOrDefault(storedFriendship => storedFriendship.Status == FriendshipStatuses.Pending);
            if (pendingRequest is null)
            {
                return false;
            }

            pendingRequest.Status = FriendshipStatuses.Accepted;
            pendingRequest.RespondedAt = DateTime.UtcNow;
            bool wasAccepted = true;
            try
            {
                databaseContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                wasAccepted = false;
            }

            return wasAccepted;
        }

        public bool DeleteRequest(int requesterId, int addresseeId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requesterId);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(addresseeId);

            using TorresDbContext databaseContext = _contextFactory.CreateDbContext();

            int deletedCount = databaseContext.Friendships
                .Where(storedFriendship => (storedFriendship.RequesterId == requesterId) && (storedFriendship.AddresseeId == addresseeId))
                .Where(storedFriendship => storedFriendship.Status == FriendshipStatuses.Pending)
                .ExecuteDelete();
            bool wasDeleted = deletedCount > 0;

            return wasDeleted;
        }
    }
}
