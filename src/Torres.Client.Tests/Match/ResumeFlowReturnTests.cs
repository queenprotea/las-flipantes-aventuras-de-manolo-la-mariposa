using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Flows;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class ResumeFlowReturnTests
    {
        private const string ReturnToMatchCall = nameof(RecordingGameSessionConnection.ReturnToMatchAsync);

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly ResumeFlow _resumeFlow;

        public ResumeFlowReturnTests()
        {
            _resumeFlow = new ResumeFlow(_gameSessionConnection, _playerSession);
        }

        [Fact]
        public void Test_Open_WithoutReturning_AsksToReturnToTheMatch()
        {
            _resumeFlow.Open();

            Assert.Contains(ReturnToMatchCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_Open_AfterTheViewerReturned_DoesNotAskAgain()
        {
            var awaitingView = new MatchView
            {
                Phase = MatchPhase.AwaitingPlayers,
                ViewerSeatNumber = SampleClients.SeatNumberOne,
                Seats = new List<MatchSeat> { new MatchSeat { SeatNumber = SampleClients.SeatNumberOne } },
            };
            _playerSession.ApplyMatchChange(new MatchChange(MatchChangeKind.StateChanged, awaitingView, DateTime.UtcNow));

            _resumeFlow.Open();

            Assert.DoesNotContain(ReturnToMatchCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_AfterReturning_ReturnsNull()
        {
            _gameSessionConnection.ReturnToMatchReply = Task.FromResult(ReturnToMatchResult.Returned);
            _resumeFlow.Open();

            ResumeOutcome? outcome = _resumeFlow.TakeOutcome();

            Assert.Null(outcome);
        }

        [Fact]
        public void Test_FindResumeTimeLeft_OutsideTheWaitForPlayers_ReturnsZero()
        {
            TimeSpan resumeTimeLeft = _resumeFlow.FindResumeTimeLeft();

            Assert.Equal(TimeSpan.Zero, resumeTimeLeft);
        }

        [Fact]
        public void Test_CurrentMatchView_OutsideAMatch_ReturnsNull()
        {
            MatchView? matchView = _resumeFlow.CurrentMatchView;

            Assert.Null(matchView);
        }
    }
}
