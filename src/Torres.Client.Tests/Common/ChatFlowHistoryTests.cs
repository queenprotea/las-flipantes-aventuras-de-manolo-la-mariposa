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
    public sealed class ChatFlowHistoryTests
    {
        private const string GetChatHistoryCall = nameof(RecordingGameSessionConnection.GetChatHistoryAsync);
        private const string Greeting = "Hola a todos";
        private const string BlankText = "   ";
        private const int BetoMessageId = 3;
        private const int AnaMemberId = 1;
        private const int BetoMemberId = 2;
        private const int MessageMaxLength = 20;

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly ChatMessage _betoMessage = new ChatMessage { RoomId = SampleClients.RoomId, MessageId = BetoMessageId, SenderMemberId = BetoMemberId, Text = Greeting };
        private readonly ChatFlow _chatFlow;

        public ChatFlowHistoryTests()
        {
            _chatFlow = new ChatFlow(_gameSessionConnection, _playerSession);
        }

        [Fact]
        public void Test_Open_InsideARoom_AsksForTheHistory()
        {
            EnterRoomOfAna();

            _chatFlow.Open();

            Assert.Contains(GetChatHistoryCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_Open_OutsideARoom_DoesNotAskForTheHistory()
        {
            _chatFlow.Open();

            Assert.DoesNotContain(GetChatHistoryCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheHistoryArrives_ShowsItsMessages()
        {
            EnterRoomOfAna();
            _gameSessionConnection.GetChatHistoryReply = Task.FromResult(CreateHistory());
            _chatFlow.Open();

            _chatFlow.TakeOutcome();

            IEnumerable<int> messageIds = _chatFlow.Messages.Select(message => message.MessageId);
            Assert.Equal(new int[] { BetoMessageId }, messageIds);
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheHistoryArrives_KeepsTheMaximumLength()
        {
            EnterRoomOfAna();
            _gameSessionConnection.GetChatHistoryReply = Task.FromResult(CreateHistory());
            _chatFlow.Open();

            _chatFlow.TakeOutcome();

            int messageMaxLength = _chatFlow.MessageMaxLength;
            Assert.Equal(MessageMaxLength, messageMaxLength);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheHistoryFails_ShowsTheServerError()
        {
            EnterRoomOfAna();
            _gameSessionConnection.GetChatHistoryReply = SampleClients.CreateFailedCallAsync<ChatHistoryResponse>();
            _chatFlow.Open();

            ChatOutcome? outcome = _chatFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.NoticeKey);
        }

        [Fact]
        public void Test_Send_WithABlankText_ReturnsFalse()
        {
            bool canSend = _chatFlow.Send(BlankText);

            Assert.False(canSend);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheServerFindsTheMessageEmpty_AcceptsTheText()
        {
            _gameSessionConnection.SendChatMessageReply = Task.FromResult(SendChatMessageResult.EmptyMessage);
            _chatFlow.Send(Greeting);

            ChatOutcome? outcome = _chatFlow.TakeOutcome();

            Assert.Equal(Greeting, outcome?.AcceptedText);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheMessageIsTooLong_ReturnsNull()
        {
            _gameSessionConnection.SendChatMessageReply = Task.FromResult(SendChatMessageResult.MessageTooLong);
            _chatFlow.Send(Greeting);

            ChatOutcome? outcome = _chatFlow.TakeOutcome();

            Assert.Null(outcome);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheReportedMessageIsGone_ShowsReportUnavailable()
        {
            _gameSessionConnection.ReportChatMessageReply = Task.FromResult(ReportChatMessageResult.MessageNotFound);
            _chatFlow.Report(_betoMessage);

            ChatOutcome? outcome = _chatFlow.TakeOutcome();

            Assert.Equal(TextKeys.Chat.ReportUnavailable, outcome?.NoticeKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenReportingOutsideTheRoom_ShowsTheServerError()
        {
            _gameSessionConnection.ReportChatMessageReply = Task.FromResult(ReportChatMessageResult.NotInRoom);
            _chatFlow.Report(_betoMessage);

            ChatOutcome? outcome = _chatFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.NoticeKey);
        }

        [Fact]
        public void Test_Report_WithTheSessionClosed_ReturnsFalse()
        {
            _gameSessionConnection.IsSessionOpen = false;

            bool canReport = _chatFlow.Report(_betoMessage);

            Assert.False(canReport);
        }

        [Fact]
        public void Test_FindReportableMessageIds_WithAMessageOfAnotherAccount_ListsIt()
        {
            EnterRoomOfAna();
            _playerSession.RoomChat.Add(_betoMessage);

            HashSet<int> reportableMessageIds = _chatFlow.FindReportableMessageIds();

            Assert.Contains(BetoMessageId, reportableMessageIds);
        }

        [Fact]
        public void Test_FindReportableMessageIds_WithAMessageOfAGuest_LeavesItOut()
        {
            EnterRoomOfAna();
            _playerSession.RoomChat.Add(new ChatMessage { RoomId = SampleClients.RoomId, MessageId = BetoMessageId, SenderMemberId = BetoMemberId, SenderIsGuest = true });

            HashSet<int> reportableMessageIds = _chatFlow.FindReportableMessageIds();

            Assert.Empty(reportableMessageIds);
        }

        private static ChatHistoryResponse CreateHistory()
        {
            var history = new ChatHistoryResponse
            {
                Messages = new List<ChatMessage> { new ChatMessage { RoomId = SampleClients.RoomId, MessageId = BetoMessageId, SenderMemberId = BetoMemberId } },
                ClosedReportMessageIds = new List<int>(),
                MessageMaxLength = MessageMaxLength,
            };

            return history;
        }

        private void EnterRoomOfAna()
        {
            Room room = SampleClients.CreateRoom();
            room.ViewerMemberId = AnaMemberId;
            _playerSession.EnterRoom(room);
        }
    }
}
