using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using CoreWCF;

using Game.Contracts;
using Game.Persistence;

using log4net;

using Npgsql;

namespace Game.Services
{
    public sealed class FriendsService : IFriendsService
    {
        private const string FriendsFeatureName = "the friends features";
        private const string FriendsUnavailableReason = "The friends of the player are not available right now.";
        private const string PlayerSearchUnavailableReason = "The player search is not available right now.";
        private const string FriendsReadFailedMessage = "The database could not be read while listing the friends of player {0}.";
        private const string PlayerSearchReadFailedMessage = "The database could not be read during a player search by player {0}.";
        private const string FriendRequestWriteFailedMessage = "The database could not be written while player {0} sent a friend request.";
        private const string FriendResponseWriteFailedMessage = "The database could not be written while player {0} answered a friend request.";
        private const string FriendRemovalWriteFailedMessage = "The database could not be written while player {0} removed a friend.";
        private const string FriendRequestSentMessage = "Player {0} sent a friend request to player {1}.";
        private const string FriendRequestAcceptedMessage = "Player {0} accepted the friend request of player {1}.";
        private const string FriendRequestRejectedMessage = "Player {0} rejected the friend request of player {1}.";
        private const string FriendRemovedMessage = "Player {0} removed player {1} from their friends.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(FriendsService));

        private readonly SessionRegistry _sessionRegistry;
        private readonly PlayerRepository _playerRepository;
        private readonly FriendshipRepository _friendshipRepository;

        public FriendsService(SessionRegistry sessionRegistry, PlayerRepository playerRepository, FriendshipRepository friendshipRepository)
        {
            ArgumentNullException.ThrowIfNull(sessionRegistry);
            ArgumentNullException.ThrowIfNull(playerRepository);
            ArgumentNullException.ThrowIfNull(friendshipRepository);

            _sessionRegistry = sessionRegistry;
            _playerRepository = playerRepository;
            _friendshipRepository = friendshipRepository;
        }

        public FriendshipListResponse GetFriendshipList()
        {
            int callerPlayerId = RequireAccountOwner().OwnerId;
            FriendshipListResponse friendshipList;
            try
            {
                friendshipList = ReadFriendshipList(callerPlayerId);
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, FriendsReadFailedMessage, callerPlayerId);
                _log.Error(failureMessage, exception);
                throw new FaultException(FriendsUnavailableReason);
            }

            return friendshipList;
        }

        public PlayerSearchResponse FindPlayerByUsername(string username)
        {
            int callerPlayerId = RequireAccountOwner().OwnerId;
            bool isBlankUsername = string.IsNullOrWhiteSpace(username);
            PlayerSearchResponse searchResult = isBlankUsername
                ? new PlayerSearchResponse { Status = PlayerSearchStatus.NotFound }
                : ReadPlayerSearch(username.Trim(), callerPlayerId);

            return searchResult;
        }

        private PlayerSearchResponse ReadPlayerSearch(string username, int callerPlayerId)
        {
            try
            {
                PlayerSearchResponse storedSearchResult = SearchPlayer(username, callerPlayerId);

                return storedSearchResult;
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, PlayerSearchReadFailedMessage, callerPlayerId);
                _log.Error(failureMessage, exception);
                throw new FaultException(PlayerSearchUnavailableReason);
            }
        }

        public FriendRequestResult SendFriendRequest(int targetPlayerId)
        {
            int callerPlayerId = RequireAccountOwner().OwnerId;
            FriendRequestResult requestResult = FriendRequestResult.PlayerNotFound;

            if ((targetPlayerId > 0) && (targetPlayerId != callerPlayerId))
            {
                requestResult = CreateFriendRequest(callerPlayerId, targetPlayerId);
            }

            return requestResult;
        }

        public FriendRequestResponseResult RespondFriendRequest(int requesterPlayerId, bool accept)
        {
            int callerPlayerId = RequireAccountOwner().OwnerId;
            if (requesterPlayerId <= 0)
            {
                return FriendRequestResponseResult.RequestNoLongerExists;
            }

            FriendRequestResponseResult responseResult;
            try
            {
                responseResult = accept
                    ? AcceptFriendRequest(requesterPlayerId, callerPlayerId)
                    : RejectFriendRequest(requesterPlayerId, callerPlayerId);
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, FriendResponseWriteFailedMessage, callerPlayerId);
                _log.Error(failureMessage, exception);
                responseResult = FriendRequestResponseResult.DatabaseUnavailable;
            }

            return responseResult;
        }

        public FriendRemovalResult RemoveFriend(int friendPlayerId)
        {
            int callerPlayerId = RequireAccountOwner().OwnerId;
            FriendRemovalResult removalResult;
            try
            {
                removalResult = DeleteFriendship(callerPlayerId, friendPlayerId);
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, FriendRemovalWriteFailedMessage, callerPlayerId);
                _log.Error(failureMessage, exception);
                removalResult = FriendRemovalResult.DatabaseUnavailable;
            }

            return removalResult;
        }

        private SessionOwner RequireAccountOwner()
        {
            SessionOwner accountOwner = AccountOwnerGuard.RequireAccountOwner(_sessionRegistry, _log, FriendsFeatureName);

            return accountOwner;
        }

        private FriendshipListResponse ReadFriendshipList(int callerPlayerId)
        {
            List<Friendship> friendships = _friendshipRepository.ListByPlayer(callerPlayerId);
            Dictionary<int, Friendship> friendshipsByOtherPlayer = friendships.ToDictionary(friendship => FindOtherPlayerId(friendship, callerPlayerId));
            List<Player> otherPlayers = _playerRepository.ListByIds(friendshipsByOtherPlayer.Keys);

            var friendshipList = new FriendshipListResponse();
            foreach (Player otherPlayer in otherPlayers)
            {
                FriendRelationship relationship = ToRelationship(friendshipsByOtherPlayer[otherPlayer.PlayerId], callerPlayerId);
                List<PlayerSummary> relationshipPlayers = SelectRelationshipPlayers(friendshipList, relationship);
                PlayerSummary otherPlayerSummary = ToPlayerSummary(otherPlayer);
                otherPlayerSummary.IsOnline = (relationship == FriendRelationship.Friend) && _sessionRegistry.HasOpenSession(otherPlayer.PlayerId);
                relationshipPlayers.Add(otherPlayerSummary);
            }

            return friendshipList;
        }

        private PlayerSearchResponse SearchPlayer(string username, int callerPlayerId)
        {
            Player? foundPlayer = _playerRepository.FindByUsername(username);
            if ((foundPlayer is null) || (foundPlayer.PlayerId == callerPlayerId))
            {
                var notFoundResult = new PlayerSearchResponse { Status = PlayerSearchStatus.NotFound };
                return notFoundResult;
            }

            Friendship? friendship = _friendshipRepository.FindBetween(callerPlayerId, foundPlayer.PlayerId);
            PlayerSummary foundSummary = ToPlayerSummary(foundPlayer);
            FriendRelationship foundRelationship = ToRelationship(friendship, callerPlayerId);
            var foundResult = new PlayerSearchResponse { Status = PlayerSearchStatus.Found, Player = foundSummary, Relationship = foundRelationship };

            return foundResult;
        }

        private FriendRequestResult CreateFriendRequest(int callerPlayerId, int targetPlayerId)
        {
            FriendRequestResult requestResult;
            try
            {
                requestResult = StoreFriendRequest(callerPlayerId, targetPlayerId);
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, FriendRequestWriteFailedMessage, callerPlayerId);
                _log.Error(failureMessage, exception);
                requestResult = FriendRequestResult.DatabaseUnavailable;
            }

            return requestResult;
        }

        private FriendRequestResult StoreFriendRequest(int callerPlayerId, int targetPlayerId)
        {
            FriendRequestCreationOutcome creationOutcome = _friendshipRepository.CreateRequest(callerPlayerId, targetPlayerId);
            FriendRequestResult requestResult = FriendRequestResult.PlayerNotFound;
            if (creationOutcome == FriendRequestCreationOutcome.Created)
            {
                requestResult = FriendRequestResult.Sent;
                _log.InfoFormat(FriendRequestSentMessage, callerPlayerId, targetPlayerId);
            }
            else if (creationOutcome == FriendRequestCreationOutcome.PairTaken)
            {
                Friendship? existingFriendship = _friendshipRepository.FindBetween(callerPlayerId, targetPlayerId);
                requestResult = ToTakenPairResult(existingFriendship);
            }

            return requestResult;
        }

        private FriendRequestResponseResult AcceptFriendRequest(int requesterPlayerId, int callerPlayerId)
        {
            bool wasAccepted = _friendshipRepository.AcceptRequest(requesterPlayerId, callerPlayerId);
            FriendRequestResponseResult responseResult = FriendRequestResponseResult.RequestNoLongerExists;
            if (wasAccepted)
            {
                responseResult = FriendRequestResponseResult.Accepted;
                _log.InfoFormat(FriendRequestAcceptedMessage, callerPlayerId, requesterPlayerId);
            }

            return responseResult;
        }

        private FriendRequestResponseResult RejectFriendRequest(int requesterPlayerId, int callerPlayerId)
        {
            bool wasRejected = _friendshipRepository.DeleteRequest(requesterPlayerId, callerPlayerId);
            FriendRequestResponseResult responseResult = FriendRequestResponseResult.RequestNoLongerExists;
            if (wasRejected)
            {
                responseResult = FriendRequestResponseResult.Rejected;
                _log.InfoFormat(FriendRequestRejectedMessage, callerPlayerId, requesterPlayerId);
            }

            return responseResult;
        }

        private FriendRemovalResult DeleteFriendship(int callerPlayerId, int friendPlayerId)
        {
            bool wasRemoved = _friendshipRepository.DeleteFriendship(callerPlayerId, friendPlayerId);
            FriendRemovalResult removalResult = FriendRemovalResult.NotFriends;
            if (wasRemoved)
            {
                removalResult = FriendRemovalResult.Removed;
                _log.InfoFormat(FriendRemovedMessage, callerPlayerId, friendPlayerId);
            }

            return removalResult;
        }

        private static int FindOtherPlayerId(Friendship friendship, int callerPlayerId)
        {
            int otherPlayerId = friendship.RequesterId == callerPlayerId ? friendship.AddresseeId : friendship.RequesterId;

            return otherPlayerId;
        }

        private static FriendRelationship ToRelationship(Friendship? friendship, int callerPlayerId)
        {
            if (friendship is null)
            {
                return FriendRelationship.None;
            }

            FriendRelationship relationship = FriendRelationship.RequestReceived;
            if (friendship.Status == FriendshipStatuses.Accepted)
            {
                relationship = FriendRelationship.Friend;
            }
            else if (friendship.RequesterId == callerPlayerId)
            {
                relationship = FriendRelationship.RequestSent;
            }

            return relationship;
        }

        private static FriendRequestResult ToTakenPairResult(Friendship? existingFriendship)
        {
            bool isAccepted = existingFriendship?.Status == FriendshipStatuses.Accepted;
            FriendRequestResult takenPairResult = isAccepted ? FriendRequestResult.AlreadyFriends : FriendRequestResult.RequestAlreadyPending;

            return takenPairResult;
        }

        private static List<PlayerSummary> SelectRelationshipPlayers(FriendshipListResponse friendshipList, FriendRelationship relationship)
        {
            List<PlayerSummary> relationshipPlayers = relationship switch
            {
                FriendRelationship.Friend => friendshipList.Friends,
                FriendRelationship.RequestSent => friendshipList.SentRequests,
                FriendRelationship.RequestReceived => friendshipList.ReceivedRequests,
                _ => throw new ArgumentOutOfRangeException(nameof(relationship), relationship, null),
            };

            return relationshipPlayers;
        }

        private static PlayerSummary ToPlayerSummary(Player player)
        {
            var playerSummary = new PlayerSummary
            {
                PlayerId = player.PlayerId,
                Username = player.Username,
                AvatarReference = player.AvatarReference,
            };

            return playerSummary;
        }
    }
}
