using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class ChatFlowTests
    {
        private const string Greeting = "Hola a todos";
        private const int ReportedMessageId = 3;
        private const int BetoMemberId = 2;

        private static readonly ChatMessage _betoMessage = new ChatMessage { RoomId = SampleClients.RoomId, MessageId = ReportedMessageId, SenderMemberId = BetoMemberId, SenderDisplayName = SampleClients.BetoUsername, Text = Greeting };

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly ChatFlow _chatFlow;

        public ChatFlowTests()
        {
            _chatFlow = new ChatFlow(_gameSessionConnection, SampleClients.CreateSessionOfAna());
        }

        [Fact]
        public void Test_TakeOutcome_AfterSending_AcceptsTheText()
        {
            _gameSessionConnection.SendChatMessageReply = Task.FromResult(SendChatMessageResult.Sent);
            _chatFlow.Send(Greeting);

            ChatOutcome? outcome = _chatFlow.TakeOutcome();

            Assert.Equal(Greeting, outcome?.AcceptedText);
        }

        [Fact]
        public void Test_TakeOutcome_WhenSendingFails_ShowsTheServerError()
        {
            _gameSessionConnection.SendChatMessageReply = SampleClients.CreateFailedCallAsync<SendChatMessageResult>();
            _chatFlow.Send(Greeting);

            ChatOutcome? outcome = _chatFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.NoticeKey);
        }

        [Fact]
        public void Test_FindReportableMessageIds_WithoutMessages_ReturnsNone()
        {
            HashSet<int> reportableMessageIds = _chatFlow.FindReportableMessageIds();

            Assert.Empty(reportableMessageIds);
        }

        [Fact]
        public void Test_TakeOutcome_AfterReporting_MarksTheMessageReported()
        {
            _gameSessionConnection.ReportChatMessageReply = Task.FromResult(ReportChatMessageResult.Reported);
            _chatFlow.Report(_betoMessage);

            ChatOutcome? outcome = _chatFlow.TakeOutcome();

            Assert.True(outcome?.IsReported);
        }

        [Fact]
        public void Test_TakeOutcome_WhenReportingFails_ShowsTheServerError()
        {
            _gameSessionConnection.ReportChatMessageReply = SampleClients.CreateFailedCallAsync<ReportChatMessageResult>();
            _chatFlow.Report(_betoMessage);

            ChatOutcome? outcome = _chatFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.NoticeKey);
        }
    }
}
