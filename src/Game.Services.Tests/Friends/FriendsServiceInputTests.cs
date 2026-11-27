using CoreWCF;

using Game.Contracts;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class FriendsServiceInputTests
    {
        private const string BlankUsername = "   ";
        private const char UsernameLetter = 'a';
        private const int NoPlayerId = 0;

        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly FriendsService _friendsService;

        public FriendsServiceInputTests()
        {
            _friendsService = new FriendsService(_callerSessions.Registry, new RecordingPlayerRepository(), new RecordingFriendshipRepository());
            _callerSessions.Open(SamplePlayers.CaroGuest);
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public void Test_FindPlayerByUsername_WithABlankUsername_ReturnsNotFound()
        {
            PlayerSearchResponse searchResult = _friendsService.FindPlayerByUsername(BlankUsername);

            Assert.Equal(PlayerSearchStatus.NotFound, searchResult.Status);
        }

        [Fact]
        public void Test_FindPlayerByUsername_WithATooLongUsername_ReturnsNotFound()
        {
            string tooLongUsername = new string(UsernameLetter, AccountLimits.LongestUsername + 1);

            PlayerSearchResponse searchResult = _friendsService.FindPlayerByUsername(tooLongUsername);

            Assert.Equal(PlayerSearchStatus.NotFound, searchResult.Status);
        }

        [Fact]
        public void Test_SendFriendRequest_ToItself_ReturnsPlayerNotFound()
        {
            FriendRequestResult requestResult = _friendsService.SendFriendRequest(SamplePlayers.AnaId);

            Assert.Equal(FriendRequestResult.PlayerNotFound, requestResult);
        }

        [Fact]
        public void Test_SendFriendRequest_WithoutPlayerId_ReturnsPlayerNotFound()
        {
            FriendRequestResult requestResult = _friendsService.SendFriendRequest(NoPlayerId);

            Assert.Equal(FriendRequestResult.PlayerNotFound, requestResult);
        }

        [Fact]
        public void Test_RespondFriendRequest_WithoutRequesterId_ReturnsRequestNoLongerExists()
        {
            FriendRequestResponseResult responseResult = _friendsService.RespondFriendRequest(NoPlayerId, true);

            Assert.Equal(FriendRequestResponseResult.RequestNoLongerExists, responseResult);
        }

        [Fact]
        public void Test_GetFriendshipList_AsAGuest_ThrowsFaultException()
        {
            _callerSessions.ActAs(SamplePlayers.CaroGuest);

            Assert.Throws<FaultException>(() => _friendsService.GetFriendshipList());
        }
    }
}
