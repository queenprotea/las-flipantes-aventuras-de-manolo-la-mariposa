using CoreWCF;

using Game.Contracts;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class FriendsServiceFailureTests
    {
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly FriendsService _friendsService;

        public FriendsServiceFailureTests()
        {
            _friendsService = new FriendsService(_callerSessions.Registry, new RecordingPlayerRepository(), new FailingFriendshipRepository());
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public void Test_GetFriendshipList_WithTheDatabaseDown_ThrowsFaultException()
        {
            Assert.Throws<FaultException>(() => _friendsService.GetFriendshipList());
        }

        [Fact]
        public void Test_FindPlayerByUsername_WithTheDatabaseDown_ThrowsFaultException()
        {
            Assert.Throws<FaultException>(() => _friendsService.FindPlayerByUsername(SamplePlayers.BetoUsername));
        }

        [Fact]
        public void Test_SendFriendRequest_WithTheDatabaseDown_ReturnsDatabaseUnavailable()
        {
            FriendRequestResult requestResult = _friendsService.SendFriendRequest(SamplePlayers.BetoId);

            Assert.Equal(FriendRequestResult.DatabaseUnavailable, requestResult);
        }

        [Fact]
        public void Test_RespondFriendRequest_AcceptingWithTheDatabaseDown_ReturnsDatabaseUnavailable()
        {
            FriendRequestResponseResult responseResult = _friendsService.RespondFriendRequest(SamplePlayers.BetoId, true);

            Assert.Equal(FriendRequestResponseResult.DatabaseUnavailable, responseResult);
        }

        [Fact]
        public void Test_RespondFriendRequest_RejectingWithTheDatabaseDown_ReturnsDatabaseUnavailable()
        {
            FriendRequestResponseResult responseResult = _friendsService.RespondFriendRequest(SamplePlayers.BetoId, false);

            Assert.Equal(FriendRequestResponseResult.DatabaseUnavailable, responseResult);
        }

        [Fact]
        public void Test_RemoveFriend_WithTheDatabaseDown_ReturnsDatabaseUnavailable()
        {
            FriendRemovalResult removalResult = _friendsService.RemoveFriend(SamplePlayers.BetoId);

            Assert.Equal(FriendRemovalResult.DatabaseUnavailable, removalResult);
        }
    }
}
