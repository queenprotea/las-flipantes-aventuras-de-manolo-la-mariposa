using System;
using System.Collections.Generic;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class PlayerSessionTests
    {
        private const int SeatNumberTwo = 2;
        private const string NewUsername = "ana_g";
        private const string AvatarReference = "avatar-ana";

        private static readonly DateTime _receivedAtUtc = new DateTime(2026, 11, 27, 12, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime _stepEndsAtUtc = new DateTime(2026, 11, 27, 12, 1, 30, DateTimeKind.Utc);
        private static readonly DateTime _lostAtUtc = new DateTime(2026, 11, 27, 12, 0, 10, DateTimeKind.Utc);
        private static readonly DateTime _reconnectionEndsAtUtc = new DateTime(2026, 11, 27, 12, 1, 10, DateTimeKind.Utc);
        private static readonly TimeSpan _stepTimeLeft = TimeSpan.FromSeconds(90);
        private static readonly TimeSpan _reconnectionTime = TimeSpan.FromSeconds(60);

        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();

        [Fact]
        public void Test_Start_WithAnAccount_UsesTheUsernameAsDisplayName()
        {
            var playerSession = new PlayerSession();

            playerSession.Start(SampleClients.Ana);

            Assert.Equal(SampleClients.AnaUsername, playerSession.DisplayName);
        }

        [Fact]
        public void Test_StartAsGuest_WithAnAlias_MarksTheSessionAsGuest()
        {
            _playerSession.StartAsGuest(SampleClients.GuestAlias);

            Assert.True(_playerSession.IsGuest);
        }

        [Fact]
        public void Test_StartAsGuest_WithAnAlias_ForgetsTheAccount()
        {
            _playerSession.StartAsGuest(SampleClients.GuestAlias);

            Assert.False(_playerSession.HasAccount);
        }

        [Fact]
        public void Test_StartAsGuest_WithAnAlias_UsesTheAliasAsDisplayName()
        {
            _playerSession.StartAsGuest(SampleClients.GuestAlias);

            Assert.Equal(SampleClients.GuestAlias, _playerSession.DisplayName);
        }

        [Fact]
        public void Test_ApplyProfile_WithANewUsername_RenamesThePlayer()
        {
            var playerSession = new PlayerSession();
            playerSession.Start(new PlayerIdentity { PlayerId = SampleClients.AnaId, Username = SampleClients.AnaUsername });

            playerSession.ApplyProfile(NewUsername, AvatarReference);

            Assert.Equal(NewUsername, playerSession.DisplayName);
        }

        [Fact]
        public void Test_ApplyProfile_WithAnAvatar_KeepsItsReference()
        {
            _playerSession.ApplyProfile(SampleClients.AnaUsername, AvatarReference);

            Assert.Equal(AvatarReference, _playerSession.AvatarReference);
        }

        [Fact]
        public void Test_EnterRoom_WithARoom_RemembersTheRoom()
        {
            Room room = SampleClients.CreateRoom();

            _playerSession.EnterRoom(room);

            Assert.Same(room, _playerSession.CurrentRoom);
        }

        [Fact]
        public void Test_EnterMatchWithoutRoom_WithARoomCode_RemembersTheCode()
        {
            _playerSession.EnterMatchWithoutRoom(SampleClients.RoomCode);

            Assert.Equal(SampleClients.RoomCode, _playerSession.MatchRoomCode);
        }

        [Fact]
        public void Test_EnterMatchWithoutRoom_AfterEnteringARoom_LeavesTheRoom()
        {
            _playerSession.EnterRoom(SampleClients.CreateRoom());

            _playerSession.EnterMatchWithoutRoom(SampleClients.RoomCode);

            Assert.Null(_playerSession.CurrentRoom);
        }

        [Fact]
        public void Test_ApplyMatchChange_WithStepTime_CalculatesWhenTheStepEnds()
        {
            MatchChange matchChange = CreateChange(MatchChangeKind.StateChanged, SampleClients.SeatNumberOne, _stepTimeLeft);

            _playerSession.ApplyMatchChange(matchChange);

            Assert.Equal(_stepEndsAtUtc, _playerSession.StepEndsAt);
        }

        [Fact]
        public void Test_ApplyMatchChange_WithoutStepTime_LeavesNoStepEnd()
        {
            MatchChange matchChange = CreateChange(MatchChangeKind.StateChanged, SampleClients.SeatNumberOne, null);

            _playerSession.ApplyMatchChange(matchChange);

            Assert.Null(_playerSession.StepEndsAt);
        }

        [Fact]
        public void Test_ApplyMatchChange_OfANewMatchAfterForfeiting_ForgetsTheForfeit()
        {
            _playerSession.ApplyMatchChange(CreateChange(MatchChangeKind.Started, SampleClients.SeatNumberOne, _stepTimeLeft));
            _playerSession.ForfeitMatch();

            _playerSession.ApplyMatchChange(CreateChange(MatchChangeKind.Started, SampleClients.SeatNumberOne, _stepTimeLeft));

            Assert.False(_playerSession.HasForfeitedMatch);
        }

        [Fact]
        public void Test_LoseConnection_OnTheStepOfTheViewer_GivesTheRestOfTheStep()
        {
            _playerSession.ApplyMatchChange(CreateChange(MatchChangeKind.StateChanged, SampleClients.SeatNumberOne, _stepTimeLeft));

            _playerSession.LoseConnection(_lostAtUtc);

            Assert.Equal(_stepEndsAtUtc, _playerSession.ReconnectionEndsAt);
        }

        [Fact]
        public void Test_LoseConnection_OnTheTurnOfAnotherSeat_GivesTheReconnectionTime()
        {
            _playerSession.ApplyMatchChange(CreateChange(MatchChangeKind.StateChanged, SeatNumberTwo, _stepTimeLeft));

            _playerSession.LoseConnection(_lostAtUtc);

            Assert.Equal(_reconnectionEndsAtUtc, _playerSession.ReconnectionEndsAt);
        }

        [Fact]
        public void Test_LoseConnection_OutsideAMatch_LeavesNoReconnectionEnd()
        {
            _playerSession.LoseConnection(_lostAtUtc);

            Assert.Null(_playerSession.ReconnectionEndsAt);
        }

        [Fact]
        public void Test_RecoverConnection_AfterLosingIt_ForgetsTheReconnectionEnd()
        {
            _playerSession.ApplyMatchChange(CreateChange(MatchChangeKind.StateChanged, SeatNumberTwo, _stepTimeLeft));
            _playerSession.LoseConnection(_lostAtUtc);

            _playerSession.RecoverConnection();

            Assert.Null(_playerSession.ReconnectionEndsAt);
        }

        [Fact]
        public void Test_ForfeitMatch_DuringAMatch_RemembersTheForfeit()
        {
            _playerSession.ApplyMatchChange(CreateChange(MatchChangeKind.Started, SampleClients.SeatNumberOne, _stepTimeLeft));

            _playerSession.ForfeitMatch();

            Assert.True(_playerSession.HasForfeitedMatch);
        }

        [Fact]
        public void Test_ForfeitMatch_DuringAMatch_LeavesTheMatch()
        {
            _playerSession.ApplyMatchChange(CreateChange(MatchChangeKind.Started, SampleClients.SeatNumberOne, _stepTimeLeft));

            _playerSession.ForfeitMatch();

            Assert.Null(_playerSession.CurrentMatchView);
        }

        [Fact]
        public void Test_LeaveRoom_InsideARoom_LeavesTheRoom()
        {
            _playerSession.EnterRoom(SampleClients.CreateRoom());

            _playerSession.LeaveRoom();

            Assert.Null(_playerSession.CurrentRoom);
        }

        [Fact]
        public void Test_End_OfALoggedInSession_LogsOut()
        {
            _playerSession.End();

            Assert.False(_playerSession.IsLoggedIn);
        }

        [Fact]
        public void Test_HasViewerReturned_WithTheViewerConnectedWhileAwaitingPlayers_ReturnsTrue()
        {
            MatchView matchView = CreateViewWithViewerSeat(MatchPhase.AwaitingPlayers, new MatchSeat { SeatNumber = SampleClients.SeatNumberOne });

            bool hasReturned = PlayerSession.HasViewerReturned(matchView);

            Assert.True(hasReturned);
        }

        [Fact]
        public void Test_HasViewerReturned_WithTheViewerDisconnected_ReturnsFalse()
        {
            MatchView matchView = CreateViewWithViewerSeat(MatchPhase.AwaitingPlayers, new MatchSeat { SeatNumber = SampleClients.SeatNumberOne, IsDisconnected = true });

            bool hasReturned = PlayerSession.HasViewerReturned(matchView);

            Assert.False(hasReturned);
        }

        [Fact]
        public void Test_IsViewerRetired_WithTheViewerRetired_ReturnsTrue()
        {
            MatchView matchView = CreateViewWithViewerSeat(MatchPhase.PlayerTurn, new MatchSeat { SeatNumber = SampleClients.SeatNumberOne, IsRetired = true });

            bool isRetired = PlayerSession.IsViewerRetired(matchView);

            Assert.True(isRetired);
        }

        [Fact]
        public void Test_IsPlayingMatch_WithAFinishedMatch_ReturnsFalse()
        {
            MatchView matchView = CreateViewWithViewerSeat(MatchPhase.Finished, new MatchSeat { SeatNumber = SampleClients.SeatNumberOne });
            _playerSession.ApplyMatchChange(new MatchChange(MatchChangeKind.StateChanged, matchView, _receivedAtUtc));

            bool isPlayingMatch = _playerSession.IsPlayingMatch;

            Assert.False(isPlayingMatch);
        }

        private static MatchChange CreateChange(MatchChangeKind changeKind, int activeSeatNumber, TimeSpan? stepTimeLeft)
        {
            var matchView = new MatchView
            {
                Phase = MatchPhase.PlayerTurn,
                ActiveSeatNumber = activeSeatNumber,
                ViewerSeatNumber = SampleClients.SeatNumberOne,
                RoomCode = SampleClients.RoomCode,
                StepTimeLeft = stepTimeLeft,
                ReconnectionTime = _reconnectionTime,
            };
            var matchChange = new MatchChange(changeKind, matchView, _receivedAtUtc);

            return matchChange;
        }

        private static MatchView CreateViewWithViewerSeat(MatchPhase phase, MatchSeat viewerSeat)
        {
            var matchView = new MatchView
            {
                Phase = phase,
                ViewerSeatNumber = SampleClients.SeatNumberOne,
                Seats = new List<MatchSeat> { viewerSeat },
            };

            return matchView;
        }
    }
}
