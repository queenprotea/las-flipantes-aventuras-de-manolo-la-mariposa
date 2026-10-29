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
        private const string SessionNotOpenReason = "The session has not been opened with a valid session token.";
        private const string FriendsUnavailableReason = "The friends of the player are not available right now.";
        private const string PlayerSearchUnavailableReason = "The player search is not available right now.";
        private const string FriendRemovalUnavailableReason = "Removing a friend is not available yet.";
        private const string FriendsReadFailedMessage = "The database could not be read while listing the friends of player {0}.";
        private const string PlayerSearchReadFailedMessage = "The database could not be read during a player search by player {0}.";
        private const string FriendRequestWriteFailedMessage = "The database could not be written while player {0} sent a friend request.";
        private const string FriendResponseWriteFailedMessage = "The database could not be written while player {0} answered a friend request.";
        private const string FriendRequestSentMessage = "Player {0} sent a friend request to player {1}.";
        private const string FriendRequestAcceptedMessage = "Player {0} accepted the friend request of player {1}.";
        private const string FriendRequestRejectedMessage = "Player {0} rejected the friend request of player {1}.";

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

        public FriendsOverviewResponse GetFriendsOverview()
        {
            int callerPlayerId = GetCallerPlayerId();
            FriendsOverviewResponse overview;
            try
            {
                overview = ReadFriendsOverview(callerPlayerId);
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, FriendsReadFailedMessage, callerPlayerId);
                _log.Error(failureMessage, exception);
                throw new FaultException(FriendsUnavailableReason);
            }

            return overview;
        }

        public PlayerSearchResponse FindPlayerByUsername(string username)
        {
            int callerPlayerId = GetCallerPlayerId();
            var searchResult = new PlayerSearchResponse { Status = PlayerSearchStatus.NotFound };
            if (string.IsNullOrWhiteSpace(username))
            {
                return searchResult;
            }

            try
            {
                searchResult = SearchPlayer(username.Trim(), callerPlayerId);
            }
            catch (NpgsqlException exception)
            {
                string failureMessage = string.Format(CultureInfo.InvariantCulture, PlayerSearchReadFailedMessage, callerPlayerId);
                _log.Error(failureMessage, exception);
                throw new FaultException(PlayerSearchUnavailableReason);
            }

            return searchResult;
        }

        public FriendRequestResult SendFriendRequest(int targetPlayerId)
        {
            int callerPlayerId = GetCallerPlayerId();
            FriendRequestResult requestResult = FriendRequestResult.PlayerNotFound;

            if ((targetPlayerId > 0) && (targetPlayerId != callerPlayerId))
            {
                requestResult = CreateFriendRequest(callerPlayerId, targetPlayerId);
            }

            return requestResult;
        }

        public FriendRequestResponseResult RespondFriendRequest(int requesterPlayerId, bool accept)
        {
            int callerPlayerId = GetCallerPlayerId();
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
            throw new FaultException(FriendRemovalUnavailableReason);
        }

        private int GetCallerPlayerId()
        {
            PlayerSession? caller = _sessionRegistry.FindCaller();
            if (caller is null)
            {
                throw new FaultException(SessionNotOpenReason);
            }

            int callerPlayerId = caller.Owner.PlayerId;

            return callerPlayerId;
        }

        private FriendsOverviewResponse ReadFriendsOverview(int callerPlayerId)
        {
            List<Friendship> friendships = _friendshipRepository.ListByPlayer(callerPlayerId);
            Dictionary<int, Friendship> friendshipsByOtherPlayer = friendships.ToDictionary(friendship => FindOtherPlayerId(friendship, callerPlayerId));
            List<Player> otherPlayers = _playerRepository.ListByIds(friendshipsByOtherPlayer.Keys);

            var overview = new FriendsOverviewResponse();
            foreach (Player otherPlayer in otherPlayers)
            {
                FriendRelationship relationship = ToRelationship(friendshipsByOtherPlayer[otherPlayer.PlayerId], callerPlayerId);
                List<PlayerSummary> overviewSection = SelectOverviewSection(overview, relationship);
                overviewSection.Add(ToPlayerSummary(otherPlayer));
            }

            return overview;
        }

        private PlayerSearchResponse SearchPlayer(string username, int callerPlayerId)
        {
            Player? foundPlayer = _playerRepository.FindByUsername(username);
            var searchResult = new PlayerSearchResponse { Status = PlayerSearchStatus.NotFound };

            if ((foundPlayer is null) || (foundPlayer.PlayerId == callerPlayerId))
            {
                return searchResult;
            }

            Friendship? friendship = _friendshipRepository.FindBetween(callerPlayerId, foundPlayer.PlayerId);
            searchResult.Status = PlayerSearchStatus.Found;
            searchResult.Player = ToPlayerSummary(foundPlayer);
            searchResult.Relationship = ToRelationship(friendship, callerPlayerId);

            return searchResult;
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

        private static List<PlayerSummary> SelectOverviewSection(FriendsOverviewResponse overview, FriendRelationship relationship)
        {
            List<PlayerSummary> overviewSection = relationship switch
            {
                FriendRelationship.Friend => overview.Friends,
                FriendRelationship.RequestSent => overview.SentRequests,
                FriendRelationship.RequestReceived => overview.ReceivedRequests,
                _ => throw new ArgumentOutOfRangeException(nameof(relationship), relationship, null),
            };

            return overviewSection;
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
