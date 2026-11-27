using System;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class DisconnectionFlowResumeTests
    {
        private const string ReopenCall = nameof(RecordingGameSessionConnection.ReopenAsync);
        private const string AbortCall = nameof(RecordingGameSessionConnection.AbortSessionChannel);
        private const string MarkSessionOpenCall = nameof(RecordingGameSessionConnection.MarkSessionOpen);
        private const int TwoAttempts = 2;
        private const int OneAttempt = 1;

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly DisconnectionFlow _disconnectionFlow;

        public DisconnectionFlowResumeTests()
        {
            _disconnectionFlow = new DisconnectionFlow(_gameSessionConnection, _playerSession);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheServerRefusesTheSession_ReportsTheRefusal()
        {
            ReopenWith(new OpenSessionResult { Status = OpenSessionStatus.InvalidSessionToken });

            DisconnectionOutcome? outcome = _disconnectionFlow.TakeOutcome();

            bool? isSessionRefused = outcome?.IsSessionRefused;
            Assert.True(isSessionRefused);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheServerRefusesTheSession_AbortsTheChannel()
        {
            ReopenWith(new OpenSessionResult { Status = OpenSessionStatus.InvalidSessionToken });

            _disconnectionFlow.TakeOutcome();

            Assert.Contains(AbortCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheSessionReopens_ReportsTheResume()
        {
            ReopenWith(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened });

            DisconnectionOutcome? outcome = _disconnectionFlow.TakeOutcome();

            bool? isResumed = outcome?.IsResumed;
            Assert.True(isResumed);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheSessionReopens_MarksTheSessionOpen()
        {
            ReopenWith(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened });

            _disconnectionFlow.TakeOutcome();

            Assert.Contains(MarkSessionOpenCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheMatchEndedWhileAway_ReportsIt()
        {
            ReopenWith(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened });

            DisconnectionOutcome? outcome = _disconnectionFlow.TakeOutcome();

            bool? hasMatchEnded = outcome?.HasMatchEndedWhileAway;
            Assert.True(hasMatchEnded);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheMatchEndedAndTheRoomClosed_LeavesTheRoom()
        {
            _playerSession.EnterRoom(SampleClients.CreateRoom());
            ReopenWith(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened });

            _disconnectionFlow.TakeOutcome();

            Assert.Null(_playerSession.CurrentRoom);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheRoomIsStillOpen_EntersTheRoom()
        {
            Room room = SampleClients.CreateRoom();
            ReopenWith(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened, CurrentRoom = room });

            _disconnectionFlow.TakeOutcome();

            Assert.Same(room, _playerSession.CurrentRoom);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheMatchAwaitsPlayers_ReportsIt()
        {
            ReopenWith(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened, CurrentMatch = new MatchSeating { RoomCode = SampleClients.RoomCode, IsAwaitingPlayers = true } });

            DisconnectionOutcome? outcome = _disconnectionFlow.TakeOutcome();

            bool? isAwaitingPlayers = outcome?.IsMatchAwaitingPlayers;
            Assert.True(isAwaitingPlayers);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheMatchGoesOnWithoutRoom_RemembersTheRoomCode()
        {
            ReopenWith(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened, CurrentMatch = new MatchSeating { RoomCode = SampleClients.RoomCode } });

            _disconnectionFlow.TakeOutcome();

            Assert.Equal(SampleClients.RoomCode, _playerSession.MatchRoomCode);
        }

        [Fact]
        public void Test_CanRetry_AfterResuming_ReturnsFalse()
        {
            ReopenWith(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened });
            _disconnectionFlow.TakeOutcome();

            bool canRetry = _disconnectionFlow.CanRetry;

            Assert.False(canRetry);
        }

        [Fact]
        public void Test_CanRetry_BeforeOpening_ReturnsTrue()
        {
            bool canRetry = _disconnectionFlow.CanRetry;

            Assert.True(canRetry);
        }

        [Fact]
        public void Test_Advance_AfterTheRetryIntervalOfAFailedAttempt_TriesAgain()
        {
            _gameSessionConnection.ReopenReply = SampleClients.CreateFailedCallAsync<OpenSessionResult>();
            _disconnectionFlow.Open();
            _disconnectionFlow.TakeOutcome();

            _disconnectionFlow.Advance(_gameSessionConnection.ReconnectionRetryInterval);

            int reopenAttempts = _gameSessionConnection.CalledMethods.Count(calledMethod => calledMethod == ReopenCall);
            Assert.Equal(TwoAttempts, reopenAttempts);
        }

        [Fact]
        public void Test_Advance_WhileWaitingForTheMatch_DoesNotTryAgain()
        {
            ReopenWith(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened, CurrentMatch = new MatchSeating { RoomCode = SampleClients.RoomCode } });
            _disconnectionFlow.TakeOutcome();

            _disconnectionFlow.Advance(_gameSessionConnection.ReconnectionRetryInterval);

            int reopenAttempts = _gameSessionConnection.CalledMethods.Count(calledMethod => calledMethod == ReopenCall);
            Assert.Equal(OneAttempt, reopenAttempts);
        }

        [Fact]
        public void Test_Retry_AfterAFailedAttempt_TriesAgain()
        {
            _gameSessionConnection.ReopenReply = SampleClients.CreateFailedCallAsync<OpenSessionResult>();
            _disconnectionFlow.Open();
            _disconnectionFlow.TakeOutcome();

            _disconnectionFlow.Retry();

            int reopenAttempts = _gameSessionConnection.CalledMethods.Count(calledMethod => calledMethod == ReopenCall);
            Assert.Equal(TwoAttempts, reopenAttempts);
        }

        [Fact]
        public void Test_ReconnectionTimeLeft_WithoutReconnectionWindow_ReturnsZero()
        {
            TimeSpan reconnectionTimeLeft = _disconnectionFlow.ReconnectionTimeLeft;

            Assert.Equal(TimeSpan.Zero, reconnectionTimeLeft);
        }

        [Fact]
        public void Test_CurrentMatchView_OutsideAMatch_ReturnsNull()
        {
            MatchView? matchView = _disconnectionFlow.CurrentMatchView;

            Assert.Null(matchView);
        }

        private void ReopenWith(OpenSessionResult opening)
        {
            _gameSessionConnection.ReopenReply = Task.FromResult(opening);
            _disconnectionFlow.Open();
        }
    }
}
