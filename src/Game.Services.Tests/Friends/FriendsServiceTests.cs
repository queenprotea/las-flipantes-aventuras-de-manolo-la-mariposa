using System;
using System.Linq;

using Game.Contracts;
using Game.Persistence;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class FriendsServiceTests
    {
        private const string UnknownUsername = "nadie_aqui";

        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly RecordingFriendshipRepository _friendshipRepository = new RecordingFriendshipRepository();
        private readonly FriendsService _friendsService;

        public FriendsServiceTests()
        {
            _friendsService = new FriendsService(_callerSessions.Registry, new RecordingPlayerRepository(), _friendshipRepository);
            _callerSessions.Open(SamplePlayers.Beto);
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public void Test_FindPlayerByUsername_WithExistingUsername_ReturnsFound()
        {
            PlayerSearchResponse searchResult = _friendsService.FindPlayerByUsername(SamplePlayers.BetoUsername);

            Assert.Equal(PlayerSearchStatus.Found, searchResult.Status);
        }

        [Fact]
        public void Test_SendFriendRequest_ToAnotherPlayer_ReturnsSent()
        {
            FriendRequestResult requestResult = _friendsService.SendFriendRequest(SamplePlayers.BetoId);

            Assert.Equal(FriendRequestResult.Sent, requestResult);
        }

        [Fact]
        public void Test_GetFriendshipList_WithoutFriends_ReturnsNoFriends()
        {
            FriendshipListResponse friendshipList = _friendsService.GetFriendshipList();

            Assert.Empty(friendshipList.Friends);
        }

        [Fact]
        public void Test_FindPlayerByUsername_WithUnknownUsername_ReturnsNotFound()
        {
            PlayerSearchResponse searchResult = _friendsService.FindPlayerByUsername(UnknownUsername);

            Assert.Equal(PlayerSearchStatus.NotFound, searchResult.Status);
        }

        [Fact]
        public void Test_SendFriendRequest_ToAFriend_ReturnsAlreadyFriends()
        {
            StoreFriendship(SamplePlayers.AnaId, SamplePlayers.BetoId, FriendshipStatuses.Accepted);

            FriendRequestResult requestResult = _friendsService.SendFriendRequest(SamplePlayers.BetoId);

            Assert.Equal(FriendRequestResult.AlreadyFriends, requestResult);
        }

        [Fact]
        public void Test_SendFriendRequest_WithRequestAlreadySent_ReturnsRequestAlreadyPending()
        {
            StoreFriendship(SamplePlayers.AnaId, SamplePlayers.BetoId, FriendshipStatuses.Pending);

            FriendRequestResult requestResult = _friendsService.SendFriendRequest(SamplePlayers.BetoId);

            Assert.Equal(FriendRequestResult.RequestAlreadyPending, requestResult);
        }

        [Fact]
        public void Test_SendFriendRequest_WithRequestAlreadyReceived_ReturnsRequestAlreadyPending()
        {
            StoreFriendship(SamplePlayers.BetoId, SamplePlayers.AnaId, FriendshipStatuses.Pending);

            FriendRequestResult requestResult = _friendsService.SendFriendRequest(SamplePlayers.BetoId);

            Assert.Equal(FriendRequestResult.RequestAlreadyPending, requestResult);
        }

        [Fact]
        public void Test_FindPlayerByUsername_WithOwnUsername_ReturnsNotFound()
        {
            PlayerSearchResponse searchResult = _friendsService.FindPlayerByUsername(SamplePlayers.AnaUsername);

            Assert.Equal(PlayerSearchStatus.NotFound, searchResult.Status);
        }

        [Fact]
        public void Test_GetFriendshipList_AfterSendingARequest_ListsItAsSent()
        {
            StoreFriendship(SamplePlayers.AnaId, SamplePlayers.BetoId, FriendshipStatuses.Pending);

            FriendshipListResponse friendshipList = _friendsService.GetFriendshipList();

            Assert.Equal(new int[] { SamplePlayers.BetoId }, friendshipList.SentRequests.Select(sentRequest => sentRequest.PlayerId));
        }

        [Fact]
        public void Test_RespondFriendRequest_Accepting_ReturnsAccepted()
        {
            StoreFriendship(SamplePlayers.BetoId, SamplePlayers.AnaId, FriendshipStatuses.Pending);

            FriendRequestResponseResult responseResult = _friendsService.RespondFriendRequest(SamplePlayers.BetoId, true);

            Assert.Equal(FriendRequestResponseResult.Accepted, responseResult);
        }

        [Fact]
        public void Test_RespondFriendRequest_Accepting_ListsTheRequesterAsFriend()
        {
            StoreFriendship(SamplePlayers.BetoId, SamplePlayers.AnaId, FriendshipStatuses.Pending);

            _friendsService.RespondFriendRequest(SamplePlayers.BetoId, true);

            FriendshipListResponse friendshipList = _friendsService.GetFriendshipList();
            Assert.Equal(new int[] { SamplePlayers.BetoId }, friendshipList.Friends.Select(friend => friend.PlayerId));
        }

        [Fact]
        public void Test_GetFriendshipList_WithoutRequests_ReturnsNoReceivedRequests()
        {
            FriendshipListResponse friendshipList = _friendsService.GetFriendshipList();

            Assert.Empty(friendshipList.ReceivedRequests);
        }

        [Fact]
        public void Test_RespondFriendRequest_Rejecting_ReturnsRejected()
        {
            StoreFriendship(SamplePlayers.BetoId, SamplePlayers.AnaId, FriendshipStatuses.Pending);

            FriendRequestResponseResult responseResult = _friendsService.RespondFriendRequest(SamplePlayers.BetoId, false);

            Assert.Equal(FriendRequestResponseResult.Rejected, responseResult);
        }

        [Fact]
        public void Test_RespondFriendRequest_WithMissingRequest_ReturnsRequestNoLongerExists()
        {
            FriendRequestResponseResult responseResult = _friendsService.RespondFriendRequest(SamplePlayers.UnknownPlayerId, true);

            Assert.Equal(FriendRequestResponseResult.RequestNoLongerExists, responseResult);
        }

        [Fact]
        public void Test_RemoveFriend_OfAFriend_ReturnsRemoved()
        {
            StoreFriendship(SamplePlayers.AnaId, SamplePlayers.BetoId, FriendshipStatuses.Accepted);

            FriendRemovalResult removalResult = _friendsService.RemoveFriend(SamplePlayers.BetoId);

            Assert.Equal(FriendRemovalResult.Removed, removalResult);
        }

        [Fact]
        public void Test_GetFriendshipList_AfterBeingRemovedByAFriend_ReturnsNoFriends()
        {
            StoreFriendship(SamplePlayers.AnaId, SamplePlayers.BetoId, FriendshipStatuses.Accepted);
            _friendsService.RemoveFriend(SamplePlayers.BetoId);
            _callerSessions.ActAs(SamplePlayers.Beto);

            FriendshipListResponse betoFriendshipList = _friendsService.GetFriendshipList();

            Assert.Empty(betoFriendshipList.Friends);
        }

        [Fact]
        public void Test_GetFriendshipList_WithAConnectedFriend_MarksItOnline()
        {
            StoreFriendship(SamplePlayers.AnaId, SamplePlayers.BetoId, FriendshipStatuses.Accepted);

            FriendshipListResponse friendshipList = _friendsService.GetFriendshipList();

            Assert.True(friendshipList.Friends[0].IsOnline);
        }

        private void StoreFriendship(int requesterId, int addresseeId, string status)
        {
            _friendshipRepository.Friendships.Add(new Friendship { RequesterId = requesterId, AddresseeId = addresseeId, Status = status, RequestedAt = DateTime.UnixEpoch });
        }
    }
}
