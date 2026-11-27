using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class ResumeFlowTests
    {
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly ResumeFlow _resumeFlow;

        public ResumeFlowTests()
        {
            _resumeFlow = new ResumeFlow(_gameSessionConnection, SampleClients.CreateSessionOfAna());
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheSeatIsGone_ReportsTheMatchLost()
        {
            _gameSessionConnection.ReturnToMatchReply = Task.FromResult(ReturnToMatchResult.NoMatchToResume);
            _resumeFlow.Open();

            ResumeOutcome? outcome = _resumeFlow.TakeOutcome();

            Assert.True(outcome?.IsMatchLost);
        }

        [Fact]
        public void Test_TakeOutcome_WhenReturningFails_ShowsTheServerError()
        {
            _gameSessionConnection.ReturnToMatchReply = SampleClients.CreateFailedCallAsync<ReturnToMatchResult>();
            _resumeFlow.Open();

            ResumeOutcome? outcome = _resumeFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }
    }
}
