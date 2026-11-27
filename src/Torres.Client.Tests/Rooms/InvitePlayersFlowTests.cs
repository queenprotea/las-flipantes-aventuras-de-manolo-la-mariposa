using System;
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
    public sealed class InvitePlayersFlowTests
    {
        private const string GetFriendshipListCall = nameof(RecordingGameSessionConnection.GetFriendshipListAsync);
        private const string FindPlayerCall = nameof(RecordingGameSessionConnection.FindPlayerByUsernameAsync);
        private const string SendInvitationByEmailCall = nameof(RecordingGameSessionConnection.SendInvitationByEmailAsync);
        private const int OneRequest = 1;
        private const int TwoRequests = 2;
        private const string BlankUsername = "   ";
        private const string AnaUsernameInCapitals = "ANA_GARCIA";
        private const string BetoUsernameInCapitals = "BETO_RUIZ";

        private static readonly TimeSpan _halfRefreshPeriod = TimeSpan.FromSeconds(2);

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly PlayerSummary _beto = new PlayerSummary { PlayerId = SampleClients.BetoId, Username = SampleClients.BetoUsername };
        private readonly InvitePlayersFlow _invitePlayersFlow;

        public InvitePlayersFlowTests()
        {
            _gameSessionConnection.GetFriendshipListReply = Task.FromResult(new FriendshipListResponse { Friends = new List<PlayerSummary> { _beto }, ReceivedRequests = new List<PlayerSummary>(), SentRequests = new List<PlayerSummary>() });
            _gameSessionConnection.GetRoomInviteesReply = Task.FromResult(new RoomInviteesResponse { InvitedPlayerIds = new List<int>() });
            _invitePlayersFlow = new InvitePlayersFlow(_gameSessionConnection, _playerSession, SampleClients.RefreshPeriod);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheInvitationFails_ShowsTheServerError()
        {
            _gameSessionConnection.SendInvitationReply = SampleClients.CreateFailedCallAsync<SendInvitationResult>();
            _invitePlayersFlow.Invite(SampleClients.BetoId, false);

            InvitePlayersOutcome? outcome = _invitePlayersFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.ErrorKey);
        }

        [Fact]
        public void Test_FindScreenToGoBack_FromTheInvitations_ReturnsRoom()
        {
            ScreenId nextScreen = InvitePlayersFlow.FindScreenToGoBack();

            Assert.Equal(ScreenId.Room, nextScreen);
        }

        [Fact]
        public void Test_Open_AsAGuest_RedirectsToTheLogin()
        {
            var guestSession = new PlayerSession();
            guestSession.StartAsGuest(SampleClients.GuestAlias);
            var guestFlow = new InvitePlayersFlow(_gameSessionConnection, guestSession, SampleClients.RefreshPeriod);

            ScreenId? redirect = guestFlow.Open();

            Assert.Equal(ScreenId.Login, redirect);
        }

        [Fact]
        public void Test_Open_WithTheSessionClosed_RedirectsToTheLogin()
        {
            _gameSessionConnection.IsSessionOpen = false;

            ScreenId? redirect = _invitePlayersFlow.Open();

            Assert.Equal(ScreenId.Login, redirect);
        }

        [Fact]
        public void Test_Open_OutsideARoom_RedirectsToTheMainMenu()
        {
            ScreenId? redirect = _invitePlayersFlow.Open();

            Assert.Equal(ScreenId.MainMenu, redirect);
        }

        [Fact]
        public void Test_Open_InsideARoom_StaysOnTheInvitations()
        {
            _playerSession.EnterRoom(CreateRoomWithAna());

            ScreenId? redirect = _invitePlayersFlow.Open();

            Assert.Null(redirect);
        }

        [Fact]
        public void Test_Open_InsideARoom_AsksForTheFriends()
        {
            _playerSession.EnterRoom(CreateRoomWithAna());

            _invitePlayersFlow.Open();

            Assert.Contains(GetFriendshipListCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheFriendsArrive_ShowsThem()
        {
            OpenInsideARoom();

            _invitePlayersFlow.TakeOutcome();

            IEnumerable<int> friendIds = _invitePlayersFlow.Friends.Select(friend => friend.PlayerId);
            Assert.Equal(new int[] { SampleClients.BetoId }, friendIds);
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheFriendsArrive_AsksToRender()
        {
            OpenInsideARoom();

            InvitePlayersOutcome? outcome = _invitePlayersFlow.TakeOutcome();

            bool? needsRender = outcome?.NeedsRender;
            Assert.True(needsRender);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheFriendsFail_ShowsTheServerError()
        {
            _gameSessionConnection.GetFriendshipListReply = SampleClients.CreateFailedCallAsync<FriendshipListResponse>();
            OpenInsideARoom();

            InvitePlayersOutcome? outcome = _invitePlayersFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.ErrorKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheInviteesFail_ShowsTheServerError()
        {
            _gameSessionConnection.GetRoomInviteesReply = SampleClients.CreateFailedCallAsync<RoomInviteesResponse>();
            OpenInsideARoom();

            InvitePlayersOutcome? outcome = _invitePlayersFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.ErrorKey);
        }

        [Fact]
        public void Test_IsInvited_OfAPlayerTheServerListsAsInvited_ReturnsTrue()
        {
            _gameSessionConnection.GetRoomInviteesReply = Task.FromResult(new RoomInviteesResponse { InvitedPlayerIds = new List<int> { SampleClients.BetoId } });
            OpenInsideARoom();
            _invitePlayersFlow.TakeOutcome();

            bool isInvited = _invitePlayersFlow.IsInvited(SampleClients.BetoId);

            Assert.True(isInvited);
        }

        [Fact]
        public void Test_Advance_BeforeTheRefreshPeriod_DoesNotAskAgain()
        {
            OpenInsideARoom();
            _invitePlayersFlow.TakeOutcome();

            _invitePlayersFlow.Advance(_halfRefreshPeriod);

            int friendshipRequests = _gameSessionConnection.CalledMethods.Count(calledMethod => calledMethod == GetFriendshipListCall);
            Assert.Equal(OneRequest, friendshipRequests);
        }

        [Fact]
        public void Test_Advance_AfterTheRefreshPeriod_AsksForTheFriendsAgain()
        {
            OpenInsideARoom();
            _invitePlayersFlow.TakeOutcome();

            _invitePlayersFlow.Advance(SampleClients.RefreshPeriod);

            int friendshipRequests = _gameSessionConnection.CalledMethods.Count(calledMethod => calledMethod == GetFriendshipListCall);
            Assert.Equal(TwoRequests, friendshipRequests);
        }

        [Fact]
        public void Test_Search_WithABlankUsername_ReturnsRequiredField()
        {
            string? errorKey = _invitePlayersFlow.Search(BlankUsername);

            Assert.Equal(TextKeys.Common.RequiredField, errorKey);
        }

        [Fact]
        public void Test_Search_OfItsOwnUsername_FindsNobody()
        {
            _invitePlayersFlow.Search(AnaUsernameInCapitals);

            Assert.Equal(PlayerSearchStatus.NotFound, _invitePlayersFlow.SearchResult?.Status);
        }

        [Fact]
        public void Test_Search_OfItsOwnUsername_DoesNotAskTheServer()
        {
            _invitePlayersFlow.Search(AnaUsernameInCapitals);

            Assert.DoesNotContain(FindPlayerCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_Search_OfAnotherPlayer_AsksTheServer()
        {
            _invitePlayersFlow.Search(SampleClients.BetoUsername);

            Assert.Contains(FindPlayerCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_AfterASearch_KeepsTheResult()
        {
            var foundBeto = new PlayerSearchResponse { Status = PlayerSearchStatus.Found, Player = _beto };
            _gameSessionConnection.FindPlayerByUsernameReply = Task.FromResult(foundBeto);
            _invitePlayersFlow.Search(SampleClients.BetoUsername);

            _invitePlayersFlow.TakeOutcome();

            Assert.Same(foundBeto, _invitePlayersFlow.SearchResult);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheSearchFails_ShowsTheServerError()
        {
            _gameSessionConnection.FindPlayerByUsernameReply = SampleClients.CreateFailedCallAsync<PlayerSearchResponse>();
            _invitePlayersFlow.Search(SampleClients.BetoUsername);

            InvitePlayersOutcome? outcome = _invitePlayersFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.ErrorKey);
        }

        [Fact]
        public void Test_Invite_WithTheSessionClosed_ReturnsFalse()
        {
            _gameSessionConnection.IsSessionOpen = false;

            bool isSent = _invitePlayersFlow.Invite(SampleClients.BetoId, false);

            Assert.False(isSent);
        }

        [Fact]
        public void Test_Invite_ByEmail_AsksTheServerToSendAnEmail()
        {
            _invitePlayersFlow.Invite(SampleClients.BetoId, true);

            Assert.Contains(SendInvitationByEmailCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_IsInvited_AfterTheInvitationWasSent_ReturnsTrue()
        {
            _gameSessionConnection.SendInvitationReply = Task.FromResult(SendInvitationResult.Sent);
            _invitePlayersFlow.Invite(SampleClients.BetoId, false);
            _invitePlayersFlow.TakeOutcome();

            bool isInvited = _invitePlayersFlow.IsInvited(SampleClients.BetoId);

            Assert.True(isInvited);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheEmailWasNotSent_ShowsTheEmailError()
        {
            _gameSessionConnection.SendInvitationByEmailReply = Task.FromResult(SendInvitationResult.EmailNotSent);
            _invitePlayersFlow.Invite(SampleClients.BetoId, true);

            InvitePlayersOutcome? outcome = _invitePlayersFlow.TakeOutcome();

            Assert.Equal(TextKeys.Alerts.EmailFailedBody, outcome?.ErrorKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenThePlayerIsAlreadyInTheRoom_AsksForTheFriendsAgain()
        {
            _gameSessionConnection.SendInvitationReply = Task.FromResult(SendInvitationResult.AlreadyInThisRoom);
            _invitePlayersFlow.Invite(SampleClients.BetoId, false);

            _invitePlayersFlow.TakeOutcome();

            Assert.Contains(GetFriendshipListCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheSearchedPlayerNoLongerExists_ForgetsTheSearch()
        {
            _gameSessionConnection.FindPlayerByUsernameReply = Task.FromResult(new PlayerSearchResponse { Status = PlayerSearchStatus.Found, Player = _beto });
            _gameSessionConnection.SendInvitationReply = Task.FromResult(SendInvitationResult.PlayerNotFound);
            _invitePlayersFlow.Search(SampleClients.BetoUsername);
            _invitePlayersFlow.TakeOutcome();
            _invitePlayersFlow.Invite(SampleClients.BetoId, false);

            _invitePlayersFlow.TakeOutcome();

            Assert.Equal(PlayerSearchStatus.NotFound, _invitePlayersFlow.SearchResult?.Status);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheInviterLeftTheRoom_GoesToTheMainMenu()
        {
            _gameSessionConnection.SendInvitationReply = Task.FromResult(SendInvitationResult.NotInRoom);
            _invitePlayersFlow.Invite(SampleClients.BetoId, false);

            InvitePlayersOutcome? outcome = _invitePlayersFlow.TakeOutcome();

            Assert.Equal(ScreenId.MainMenu, outcome?.NextScreen);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheDatabaseIsUnavailable_ShowsTheServerError()
        {
            _gameSessionConnection.SendInvitationReply = Task.FromResult(SendInvitationResult.DatabaseUnavailable);
            _invitePlayersFlow.Invite(SampleClients.BetoId, false);

            InvitePlayersOutcome? outcome = _invitePlayersFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.ErrorKey);
        }

        [Fact]
        public void Test_IsRoomMember_OfAMemberWrittenInCapitals_ReturnsTrue()
        {
            _playerSession.EnterRoom(CreateRoomWithAna());

            bool isRoomMember = _invitePlayersFlow.IsRoomMember(AnaUsernameInCapitals);

            Assert.True(isRoomMember);
        }

        [Fact]
        public void Test_IsRoomMember_OfAPlayerOutsideTheRoom_ReturnsFalse()
        {
            _playerSession.EnterRoom(CreateRoomWithAna());

            bool isRoomMember = _invitePlayersFlow.IsRoomMember(BetoUsernameInCapitals);

            Assert.False(isRoomMember);
        }

        [Fact]
        public void Test_RoomCode_InsideARoom_ReturnsItsCode()
        {
            _playerSession.EnterRoom(CreateRoomWithAna());

            string? roomCode = _invitePlayersFlow.RoomCode;

            Assert.Equal(SampleClients.RoomCode, roomCode);
        }

        private static Room CreateRoomWithAna()
        {
            Room room = SampleClients.CreateRoom();
            room.Members = new List<RoomMember> { new RoomMember { MemberId = SampleClients.AnaId, DisplayName = SampleClients.AnaUsername } };

            return room;
        }

        private void OpenInsideARoom()
        {
            _playerSession.EnterRoom(CreateRoomWithAna());
            _invitePlayersFlow.Open();
        }
    }
}
