using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class FriendsFlowTests
    {
        private const string GetFriendshipListCall = nameof(RecordingGameSessionConnection.GetFriendshipListAsync);

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly FriendsFlow _friendsFlow;

        public FriendsFlowTests()
        {
            _friendsFlow = new FriendsFlow(_gameSessionConnection, SampleClients.CreateSessionOfAna(), SampleClients.RefreshPeriod);
        }

        [Fact]
        public void Test_Search_WithOwnUsername_ShowsNotFound()
        {
            _friendsFlow.Search(SampleClients.AnaUsername);

            Assert.Equal(PlayerSearchStatus.NotFound, _friendsFlow.SearchResult?.Status);
        }

        [Fact]
        public void Test_Search_WithOwnUsername_DoesNotCallTheServer()
        {
            _friendsFlow.Search(SampleClients.AnaUsername);

            Assert.Empty(_gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheSearchFails_ReportsTheFailure()
        {
            _gameSessionConnection.FindPlayerByUsernameReply = SampleClients.CreateFailedCallAsync<PlayerSearchResponse>();
            _friendsFlow.Search(SampleClients.BetoUsername);

            FriendsOutcome? outcome = _friendsFlow.TakeOutcome();

            Assert.True(outcome?.HasFailed);
        }

        [Fact]
        public void Test_TakeOutcome_AfterAcceptingARequest_RefreshesTheFriendList()
        {
            _gameSessionConnection.AcceptFriendRequestReply = Task.FromResult(FriendRequestResponseResult.Accepted);
            _friendsFlow.AcceptFriendRequest(SampleClients.BetoId);

            _friendsFlow.TakeOutcome();

            Assert.Contains(GetFriendshipListCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_WhenAcceptingFails_ReportsTheFailure()
        {
            _gameSessionConnection.AcceptFriendRequestReply = SampleClients.CreateFailedCallAsync<FriendRequestResponseResult>();
            _friendsFlow.AcceptFriendRequest(SampleClients.BetoId);

            FriendsOutcome? outcome = _friendsFlow.TakeOutcome();

            Assert.True(outcome?.HasFailed);
        }

        [Fact]
        public void Test_TakeOutcome_WhenRemovingFails_ReportsTheFailure()
        {
            _gameSessionConnection.RemoveFriendReply = SampleClients.CreateFailedCallAsync<FriendRemovalResult>();
            _friendsFlow.RemoveFriend(SampleClients.BetoId);

            FriendsOutcome? outcome = _friendsFlow.TakeOutcome();

            Assert.True(outcome?.HasFailed);
        }

        [Fact]
        public void Test_FindScreenToGoBack_FromTheFriends_ReturnsMainMenu()
        {
            ScreenId nextScreen = FriendsFlow.FindScreenToGoBack();

            Assert.Equal(ScreenId.MainMenu, nextScreen);
        }
    }
}
