using System;
using System.Collections.Generic;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal sealed class RecordingFriendshipRepository : IFriendshipRepository
    {
        internal List<Friendship> Friendships { get; } = new List<Friendship>();

        public List<Friendship> ListByPlayer(int playerId)
        {
            List<Friendship> playerFriendships = Friendships.FindAll(friendship => (friendship.RequesterId == playerId) || (friendship.AddresseeId == playerId));

            return playerFriendships;
        }

        public Friendship? FindBetween(int firstPlayerId, int secondPlayerId)
        {
            Friendship? foundFriendship = Friendships.Find(friendship => IsPair(friendship, firstPlayerId, secondPlayerId));

            return foundFriendship;
        }

        public FriendRequestCreationOutcome CreateRequest(int requesterId, int addresseeId)
        {
            FriendRequestCreationOutcome creationOutcome = FriendRequestCreationOutcome.PairTaken;
            if (FindBetween(requesterId, addresseeId) is null)
            {
                Friendships.Add(new Friendship { RequesterId = requesterId, AddresseeId = addresseeId, Status = FriendshipStatuses.Pending, RequestedAt = DateTime.UnixEpoch });
                creationOutcome = FriendRequestCreationOutcome.Created;
            }

            return creationOutcome;
        }

        public bool AcceptRequest(int requesterId, int addresseeId)
        {
            Friendship? pendingRequest = FindPending(requesterId, addresseeId);
            if (pendingRequest is not null)
            {
                pendingRequest.Status = FriendshipStatuses.Accepted;
            }

            bool wasAccepted = pendingRequest is not null;

            return wasAccepted;
        }

        public bool DeleteRequest(int requesterId, int addresseeId)
        {
            Friendship? pendingRequest = FindPending(requesterId, addresseeId);
            bool wasDeleted = (pendingRequest is not null) && Friendships.Remove(pendingRequest);

            return wasDeleted;
        }

        public bool DeleteFriendship(int firstPlayerId, int secondPlayerId)
        {
            Friendship? acceptedFriendship = Friendships.Find(friendship => IsPair(friendship, firstPlayerId, secondPlayerId) && (friendship.Status == FriendshipStatuses.Accepted));
            bool wasDeleted = (acceptedFriendship is not null) && Friendships.Remove(acceptedFriendship);

            return wasDeleted;
        }

        private static bool IsPair(Friendship friendship, int firstPlayerId, int secondPlayerId)
        {
            bool isPair = ((friendship.RequesterId == firstPlayerId) && (friendship.AddresseeId == secondPlayerId))
                || ((friendship.RequesterId == secondPlayerId) && (friendship.AddresseeId == firstPlayerId));

            return isPair;
        }

        private Friendship? FindPending(int requesterId, int addresseeId)
        {
            Friendship? pendingRequest = Friendships.Find(friendship => (friendship.RequesterId == requesterId) && (friendship.AddresseeId == addresseeId) && (friendship.Status == FriendshipStatuses.Pending));

            return pendingRequest;
        }
    }
}
