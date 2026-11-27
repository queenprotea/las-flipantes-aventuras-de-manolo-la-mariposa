using System;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Persistence;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class ChatServiceTests
    {
        private const int MessageMaxLength = 20;
        private const int MessagesKeptPerRoom = 50;
        private const int ThresholdOccasions = 5;
        private const int NoRoomId = 0;
        private const string Greeting = "Hola a todos";
        private const string TooLongMessage = "Este mensaje es demasiado largo";
        private const string OnlySpaces = "     ";
        private const string EarlierReportText = "texto reportado antes";

        private static readonly TimeSpan _firstBanLength = TimeSpan.FromDays(1);
        private static readonly TimeSpan _noticeWaitLimit = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan[] _temporaryBanDurations = new TimeSpan[] { _firstBanLength };
        private static readonly int[] _earlierReporterIds = new int[] { SamplePlayers.DaniId, SamplePlayers.EvaId, SamplePlayers.DaniId, SamplePlayers.EvaId };

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly RoomRegistry _roomRegistry = new RoomRegistry();
        private readonly RecordingReportRepository _reportRepository = new RecordingReportRepository();
        private readonly RecordingSanctionRepository _sanctionRepository = new RecordingSanctionRepository();
        private readonly RoomService _roomService;
        private readonly ChatService _chatService;

        public ChatServiceTests()
        {
            _roomService = new RoomService(_roomRegistry, _callerSessions.Registry, new RecordingRoomRecordRepository(), _sanctionRepository, new RecordingPlayerRepository(), _fakeTimeProvider);
            var reportRecorder = new ReportRecorder(_reportRepository, _sanctionRepository, _fakeTimeProvider);
            reportRecorder.Start(new SanctionPolicy(ThresholdOccasions, _temporaryBanDurations));
            _chatService = new ChatService(_roomRegistry, _callerSessions.Registry, reportRecorder, _fakeTimeProvider);
            _chatService.Start(new ChatCapacity(MessageMaxLength, MessagesKeptPerRoom));
            _callerSessions.Open(SamplePlayers.Beto);
            _callerSessions.Open(SamplePlayers.CaroGuest);
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public async Task Test_SendChatMessage_InTheRoom_ReturnsSent()
        {
            await OpenRoomWithBetoAndCaroAsync();

            SendChatMessageResult sendResult = _chatService.SendChatMessage(Greeting);

            Assert.Equal(SendChatMessageResult.Sent, sendResult);
        }

        [Fact]
        public async Task Test_SendChatMessage_InTheRoom_DeliversItToTheOtherMembers()
        {
            await OpenRoomWithBetoAndCaroAsync();

            _chatService.SendChatMessage(Greeting);

            ChatMessage deliveredMessage = await _callerSessions.FindReceiverOf(SamplePlayers.Beto).FirstChatMessage.WaitAsync(_noticeWaitLimit);
            Assert.Equal(Greeting, deliveredMessage.Text);
        }

        [Fact]
        public async Task Test_GetChatHistory_OfANewRoom_ReturnsNoMessages()
        {
            await OpenRoomWithBetoAndCaroAsync();

            ChatHistoryResponse history = _chatService.GetChatHistory();

            Assert.Empty(history.Messages);
        }

        [Fact]
        public async Task Test_SendChatMessage_LongerThanTheLimit_ReturnsMessageTooLong()
        {
            await OpenRoomWithBetoAndCaroAsync();

            SendChatMessageResult sendResult = _chatService.SendChatMessage(TooLongMessage);

            Assert.Equal(SendChatMessageResult.MessageTooLong, sendResult);
        }

        [Fact]
        public async Task Test_SendChatMessage_WithOnlySpaces_ReturnsEmptyMessage()
        {
            await OpenRoomWithBetoAndCaroAsync();

            SendChatMessageResult sendResult = _chatService.SendChatMessage(OnlySpaces);

            Assert.Equal(SendChatMessageResult.EmptyMessage, sendResult);
        }

        [Fact]
        public async Task Test_SendChatMessage_DuringAMatch_ReturnsSent()
        {
            int roomId = await OpenRoomWithBetoAndCaroAsync();
            _roomRegistry.FindById(roomId)?.StartMatch(SamplePlayers.AnaId);

            SendChatMessageResult sendResult = _chatService.SendChatMessage(Greeting);

            Assert.Equal(SendChatMessageResult.Sent, sendResult);
        }

        [Fact]
        public async Task Test_SendChatMessage_ByAGuest_ReturnsSent()
        {
            await OpenRoomWithBetoAndCaroAsync();
            _callerSessions.ActAs(SamplePlayers.CaroGuest);

            SendChatMessageResult sendResult = _chatService.SendChatMessage(Greeting);

            Assert.Equal(SendChatMessageResult.Sent, sendResult);
        }

        [Fact]
        public async Task Test_ReportChatMessageAsync_OfAnotherPlayersMessage_ReturnsReported()
        {
            int roomId = await OpenRoomWithBetoAndCaroAsync();
            int messageId = PostAndFindMessageId(SamplePlayers.Beto);

            ReportChatMessageResult reportResult = await _chatService.ReportChatMessageAsync(roomId, messageId);

            Assert.Equal(ReportChatMessageResult.Reported, reportResult);
        }

        [Fact]
        public async Task Test_ReportChatMessageAsync_OfAnotherPlayersMessage_StoresTheReport()
        {
            int roomId = await OpenRoomWithBetoAndCaroAsync();
            int messageId = PostAndFindMessageId(SamplePlayers.Beto);

            await _chatService.ReportChatMessageAsync(roomId, messageId);

            Assert.Equal(new int[] { SamplePlayers.BetoId }, _reportRepository.Reports.Select(report => report.ReportedPlayerId));
        }

        [Fact]
        public async Task Test_ReportChatMessageAsync_OfAGuestsMessage_ReturnsAuthorIsGuest()
        {
            int roomId = await OpenRoomWithBetoAndCaroAsync();
            int messageId = PostAndFindMessageId(SamplePlayers.CaroGuest);

            ReportChatMessageResult reportResult = await _chatService.ReportChatMessageAsync(roomId, messageId);

            Assert.Equal(ReportChatMessageResult.AuthorIsGuest, reportResult);
        }

        [Fact]
        public async Task Test_ReportChatMessageAsync_AsAGuest_ReturnsReporterIsGuest()
        {
            int roomId = await OpenRoomWithBetoAndCaroAsync();
            int messageId = PostAndFindMessageId(SamplePlayers.Beto);
            _callerSessions.ActAs(SamplePlayers.CaroGuest);

            ReportChatMessageResult reportResult = await _chatService.ReportChatMessageAsync(roomId, messageId);

            Assert.Equal(ReportChatMessageResult.ReporterIsGuest, reportResult);
        }

        [Fact]
        public async Task Test_ReportChatMessageAsync_Twice_ReturnsReported()
        {
            int roomId = await OpenRoomWithBetoAndCaroAsync();
            int messageId = PostAndFindMessageId(SamplePlayers.Beto);
            await _chatService.ReportChatMessageAsync(roomId, messageId);

            ReportChatMessageResult secondResult = await _chatService.ReportChatMessageAsync(roomId, messageId);

            Assert.Equal(ReportChatMessageResult.Reported, secondResult);
        }

        [Fact]
        public async Task Test_ReportChatMessageAsync_Twice_StoresOneReport()
        {
            int roomId = await OpenRoomWithBetoAndCaroAsync();
            int messageId = PostAndFindMessageId(SamplePlayers.Beto);
            await _chatService.ReportChatMessageAsync(roomId, messageId);

            await _chatService.ReportChatMessageAsync(roomId, messageId);

            Assert.Single(_reportRepository.Reports);
        }

        [Fact]
        public async Task Test_ReportChatMessageAsync_OutsideAnyMatch_StoresTheRoomOfTheMessage()
        {
            int roomId = await OpenRoomWithBetoAndCaroAsync();
            int messageId = PostAndFindMessageId(SamplePlayers.Beto);

            await _chatService.ReportChatMessageAsync(roomId, messageId);

            Assert.Equal(new int[] { roomId }, _reportRepository.Reports.Select(report => report.RoomId));
        }

        [Fact]
        public async Task Test_ReportChatMessageAsync_ReachingTheThreshold_CreatesASanction()
        {
            int roomId = await OpenRoomWithBetoAndCaroAsync();
            int messageId = PostAndFindMessageId(SamplePlayers.Beto);
            _reportRepository.Reports.AddRange(_earlierReporterIds.Select(ToEarlierReportOnBeto));

            await _chatService.ReportChatMessageAsync(roomId, messageId);

            Assert.Single(_sanctionRepository.Sanctions);
        }

        [Fact]
        public async Task Test_ReportChatMessageAsync_BelowTheThreshold_CreatesNoSanction()
        {
            int roomId = await OpenRoomWithBetoAndCaroAsync();
            int messageId = PostAndFindMessageId(SamplePlayers.Beto);

            await _chatService.ReportChatMessageAsync(roomId, messageId);

            Assert.Empty(_sanctionRepository.Sanctions);
        }

        private async Task<int> OpenRoomWithBetoAndCaroAsync()
        {
            _callerSessions.ActAs(SamplePlayers.Ana);
            CreateRoomResult creation = await _roomService.CreateRoomAsync(RoomVisibility.Private);
            int roomId = creation.Room?.RoomId ?? NoRoomId;
            _callerSessions.ActAs(SamplePlayers.Beto);
            await _roomService.JoinRoomAsync(roomId);
            _callerSessions.ActAs(SamplePlayers.CaroGuest);
            await _roomService.JoinRoomAsync(roomId);
            _callerSessions.ActAs(SamplePlayers.Ana);

            return roomId;
        }

        private int PostAndFindMessageId(SessionOwner author)
        {
            _callerSessions.ActAs(author);
            _chatService.SendChatMessage(Greeting);
            _callerSessions.ActAs(SamplePlayers.Ana);
            int messageId = _chatService.GetChatHistory().Messages[0].MessageId;

            return messageId;
        }

        private Report ToEarlierReportOnBeto(int reporterId)
        {
            var earlierReport = new Report
            {
                ReporterPlayerId = reporterId,
                ReportedPlayerId = SamplePlayers.BetoId,
                ReportedText = EarlierReportText,
                CreatedAt = _fakeTimeProvider.GetUtcNow().UtcDateTime,
            };

            return earlierReport;
        }
    }
}
