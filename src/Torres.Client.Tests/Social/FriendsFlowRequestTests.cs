using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class FriendsFlowRequestTests
    {
        private const string GetFriendshipListCall = nameof(RecordingGameSessionConnection.GetFriendshipListAsync);
        private const string FindPlayerCall = nameof(RecordingGameSessionConnection.FindPlayerByUsernameAsync);
        private const string SendFriendRequestCall = nameof(RecordingGameSessionConnection.SendFriendRequestAsync);
        private const int TwoRequests = 2;
        private const string BlankUsername = "   ";

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSummary _beto = new PlayerSummary { PlayerId = SampleClients.BetoId, Username = SampleClients.BetoUsername };
        private readonly FriendsFlow _friendsFlow;

        public FriendsFlowRequestTests()
        {
            _gameSessionConnection.GetFriendshipListReply = Task.FromResult(new FriendshipListResponse { Friends = new List<PlayerSummary> { _beto }, ReceivedRequests = new List<PlayerSummary>(), SentRequests = new List<PlayerSummary>() });
            _gameSessionConnection.FindPlayerByUsernameReply = Task.FromResult(new PlayerSearchResponse { Status = PlayerSearchStatus.Found, Player = _beto });
            _friendsFlow = new FriendsFlow(_gameSessionConnection, SampleClients.CreateSessionOfAna(), SampleClients.RefreshPeriod);
        }

        [Fact]
        public void Test_Open_AsAGuest_ReturnsFalse()
        {
            var guestSession = new PlayerSession();
            guestSession.StartAsGuest(SampleClients.GuestAlias);
            var guestFlow = new FriendsFlow(_gameSessionConnection, guestSession, SampleClients.RefreshPeriod);

            bool canOpen = guestFlow.Open();

            Assert.False(canOpen);
        }

        [Fact]
        public void Test_Open_WithTheSessionClosed_ReturnsFalse()
        {
            _gameSessionConnection.IsSessionOpen = false;

            bool canOpen = _friendsFlow.Open();

            Assert.False(canOpen);
        }

        [Fact]
        public void Test_Open_WithAnAccount_AsksForTheFriends()
        {
            _friendsFlow.Open();

            Assert.Contains(GetFriendshipListCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheFriendsArrive_ShowsThem()
        {
            _friendsFlow.Open();

            _friendsFlow.TakeOutcome();

            IEnumerable<int> friendIds = _friendsFlow.FriendshipList.Friends.Select(friend => friend.PlayerId);
            Assert.Equal(new int[] { SampleClients.BetoId }, friendIds);
        }

        [Fact]
        public void Test_TakeOutcome_AfterNewFriendsArrive_AsksToRender()
        {
            _friendsFlow.Open();

            FriendsOutcome? outcome = _friendsFlow.TakeOutcome();

            bool? needsRender = outcome?.NeedsRender;
            Assert.True(needsRender);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheFriendsFail_ReportsTheFailure()
        {
            _gameSessionConnection.GetFriendshipListReply = SampleClients.CreateFailedCallAsync<FriendshipListResponse>();
            _friendsFlow.Open();

            FriendsOutcome? outcome = _friendsFlow.TakeOutcome();

            bool? hasFailed = outcome?.HasFailed;
            Assert.True(hasFailed);
        }

        [Fact]
        public void Test_Advance_AfterTheRefreshPeriod_AsksForTheFriendsAgain()
        {
            _friendsFlow.Open();
            _friendsFlow.TakeOutcome();

            _friendsFlow.Advance(SampleClients.RefreshPeriod);

            int friendshipRequests = _gameSessionConnection.CalledMethods.Count(calledMethod => calledMethod == GetFriendshipListCall);
            Assert.Equal(TwoRequests, friendshipRequests);
        }

        [Fact]
        public void Test_Search_WithABlankUsername_ReturnsRequiredField()
        {
            string? errorKey = _friendsFlow.Search(BlankUsername);

            Assert.Equal(TextKeys.Common.RequiredField, errorKey);
        }

        [Fact]
        public void Test_Search_OfAnotherPlayer_AsksTheServer()
        {
            _friendsFlow.Search(SampleClients.BetoUsername);

            Assert.Contains(FindPlayerCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_AfterASearch_KeepsTheFoundPlayer()
        {
            _friendsFlow.Search(SampleClients.BetoUsername);

            _friendsFlow.TakeOutcome();

            Assert.Equal(SampleClients.BetoId, _friendsFlow.SearchResult?.Player?.PlayerId);
        }

        [Fact]
        public void Test_ClearSearch_AfterASearch_ForgetsTheResult()
        {
            _friendsFlow.Search(SampleClients.BetoUsername);
            _friendsFlow.TakeOutcome();

            _friendsFlow.ClearSearch();

            Assert.Null(_friendsFlow.SearchResult);
        }

        [Fact]
        public void Test_ClearSearch_WithoutPendingList_AsksForTheFriends()
        {
            _friendsFlow.ClearSearch();

            Assert.Contains(GetFriendshipListCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_SendFriendRequest_WithTheSessionClosed_DoesNotAskTheServer()
        {
            _gameSessionConnection.IsSessionOpen = false;

            _friendsFlow.SendFriendRequest(SampleClients.BetoId);

            Assert.DoesNotContain(SendFriendRequestCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheRequestWasSent_ShowsTheSentRequests()
        {
            _gameSessionConnection.SendFriendRequestReply = Task.FromResult(FriendRequestResult.Sent);
            _friendsFlow.SendFriendRequest(SampleClients.BetoId);

            FriendsOutcome? outcome = _friendsFlow.TakeOutcome();

            bool? showsSentRequests = outcome?.ShowsSentRequests;
            Assert.True(showsSentRequests);
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheRequestWasSent_ForgetsTheSearch()
        {
            _gameSessionConnection.SendFriendRequestReply = Task.FromResult(FriendRequestResult.Sent);
            _friendsFlow.Search(SampleClients.BetoUsername);
            _friendsFlow.TakeOutcome();
            _friendsFlow.SendFriendRequest(SampleClients.BetoId);

            _friendsFlow.TakeOutcome();

            Assert.Null(_friendsFlow.SearchResult);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheyAreAlreadyFriends_SearchesThePlayerAgain()
        {
            _gameSessionConnection.SendFriendRequestReply = Task.FromResult(FriendRequestResult.AlreadyFriends);
            _friendsFlow.Search(SampleClients.BetoUsername);
            _friendsFlow.TakeOutcome();
            _friendsFlow.SendFriendRequest(SampleClients.BetoId);

            _friendsFlow.TakeOutcome();

            int searchRequests = _gameSessionConnection.CalledMethods.Count(calledMethod => calledMethod == FindPlayerCall);
            Assert.Equal(TwoRequests, searchRequests);
        }

        [Fact]
        public void Test_TakeOutcome_WhenThePlayerNoLongerExists_FindsNobody()
        {
            _gameSessionConnection.SendFriendRequestReply = Task.FromResult(FriendRequestResult.PlayerNotFound);
            _friendsFlow.SendFriendRequest(SampleClients.BetoId);

            _friendsFlow.TakeOutcome();

            Assert.Equal(PlayerSearchStatus.NotFound, _friendsFlow.SearchResult?.Status);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheRequestFindsTheDatabaseUnavailable_ReportsTheFailure()
        {
            _gameSessionConnection.SendFriendRequestReply = Task.FromResult(FriendRequestResult.DatabaseUnavailable);
            _friendsFlow.SendFriendRequest(SampleClients.BetoId);

            FriendsOutcome? outcome = _friendsFlow.TakeOutcome();

            bool? hasFailed = outcome?.HasFailed;
            Assert.True(hasFailed);
        }

        [Fact]
        public void Test_TakeOutcome_WhenSendingTheRequestFails_ReportsTheFailure()
        {
            _gameSessionConnection.SendFriendRequestReply = SampleClients.CreateFailedCallAsync<FriendRequestResult>();
            _friendsFlow.SendFriendRequest(SampleClients.BetoId);

            FriendsOutcome? outcome = _friendsFlow.TakeOutcome();

            bool? hasFailed = outcome?.HasFailed;
            Assert.True(hasFailed);
        }

        [Fact]
        public void Test_TakeOutcome_AfterRejectingARequest_AsksForTheFriendsAgain()
        {
            _gameSessionConnection.RejectFriendRequestReply = Task.FromResult(FriendRequestResponseResult.Rejected);
            _friendsFlow.RejectFriendRequest(SampleClients.BetoId);

            _friendsFlow.TakeOutcome();

            Assert.Contains(GetFriendshipListCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_WhenRejectingFindsTheDatabaseUnavailable_ReportsTheFailure()
        {
            _gameSessionConnection.RejectFriendRequestReply = Task.FromResult(FriendRequestResponseResult.DatabaseUnavailable);
            _friendsFlow.RejectFriendRequest(SampleClients.BetoId);

            FriendsOutcome? outcome = _friendsFlow.TakeOutcome();

            bool? hasFailed = outcome?.HasFailed;
            Assert.True(hasFailed);
        }

        [Fact]
        public void Test_TakeOutcome_AfterRemovingAFriend_AsksForTheFriendsAgain()
        {
            _gameSessionConnection.RemoveFriendReply = Task.FromResult(FriendRemovalResult.Removed);
            _friendsFlow.RemoveFriend(SampleClients.BetoId);

            _friendsFlow.TakeOutcome();

            Assert.Contains(GetFriendshipListCall, _gameSessionConnection.CalledMethods);
        }
    }
}
