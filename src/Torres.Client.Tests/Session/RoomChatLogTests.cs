using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class RoomChatLogTests
    {
        private const int OtherRoomId = 31;
        private const int FirstMessageId = 1;
        private const int SecondMessageId = 2;
        private const int ThirdMessageId = 3;
        private const int TwoMessagesKept = 2;

        private readonly RoomChatLog _roomChat = new RoomChatLog();

        public RoomChatLogTests()
        {
            _roomChat.Follow(SampleClients.RoomId);
        }

        [Fact]
        public void Test_Follow_OfAnotherRoom_ChangesTheFollowedRoom()
        {
            _roomChat.Follow(OtherRoomId);

            int? followedRoomId = _roomChat.RoomId;
            Assert.Equal(OtherRoomId, followedRoomId);
        }

        [Fact]
        public void Test_Load_OfTheFollowedRoom_MarksTheChatLoaded()
        {
            _roomChat.Load(SampleClients.RoomId, CreateHistory(new int[] { FirstMessageId }));

            bool isLoaded = _roomChat.IsLoaded;
            Assert.True(isLoaded);
        }

        [Fact]
        public void Test_Load_OfAnotherRoom_LeavesTheChatUnloaded()
        {
            _roomChat.Load(OtherRoomId, CreateHistory(new int[] { FirstMessageId }));

            bool isLoaded = _roomChat.IsLoaded;
            Assert.False(isLoaded);
        }

        [Fact]
        public void Test_Load_WithMessagesOutOfOrder_SortsThem()
        {
            _roomChat.Load(SampleClients.RoomId, CreateHistory(new int[] { ThirdMessageId, FirstMessageId, SecondMessageId }));

            IEnumerable<int> messageIds = _roomChat.Messages.Select(message => message.MessageId);
            Assert.Equal(new int[] { FirstMessageId, SecondMessageId, ThirdMessageId }, messageIds);
        }

        [Fact]
        public void Test_Load_WithMoreMessagesThanKept_KeepsTheNewest()
        {
            ChatHistoryResponse history = CreateHistory(new int[] { FirstMessageId, SecondMessageId, ThirdMessageId });
            history.MessagesKeptPerRoom = TwoMessagesKept;

            _roomChat.Load(SampleClients.RoomId, history);

            IEnumerable<int> messageIds = _roomChat.Messages.Select(message => message.MessageId);
            Assert.Equal(new int[] { SecondMessageId, ThirdMessageId }, messageIds);
        }

        [Fact]
        public void Test_Add_OfAMessageAlreadyKept_KeepsOnlyOne()
        {
            _roomChat.Add(CreateMessage(SampleClients.RoomId, FirstMessageId));

            _roomChat.Add(CreateMessage(SampleClients.RoomId, FirstMessageId));

            Assert.Single(_roomChat.Messages);
        }

        [Fact]
        public void Test_Add_OfAnotherRoom_IgnoresTheMessage()
        {
            _roomChat.Add(CreateMessage(OtherRoomId, FirstMessageId));

            Assert.Empty(_roomChat.Messages);
        }

        [Fact]
        public void Test_CloseReport_OfAMessage_MarksItsReportClosed()
        {
            _roomChat.CloseReport(SampleClients.RoomId, FirstMessageId);

            bool isClosed = _roomChat.IsReportClosed(FirstMessageId);
            Assert.True(isClosed);
        }

        [Fact]
        public void Test_CloseReport_OfAnotherRoom_LeavesTheReportOpen()
        {
            _roomChat.CloseReport(OtherRoomId, FirstMessageId);

            bool isClosed = _roomChat.IsReportClosed(FirstMessageId);
            Assert.False(isClosed);
        }

        [Fact]
        public void Test_Clear_OfAFollowedRoom_StopsFollowingIt()
        {
            _roomChat.Clear();

            int? followedRoomId = _roomChat.RoomId;
            Assert.Null(followedRoomId);
        }

        private static ChatMessage CreateMessage(int roomId, int messageId)
        {
            var message = new ChatMessage { RoomId = roomId, MessageId = messageId };

            return message;
        }

        private static ChatHistoryResponse CreateHistory(int[] messageIds)
        {
            var history = new ChatHistoryResponse
            {
                Messages = messageIds.Select(messageId => CreateMessage(SampleClients.RoomId, messageId)).ToList(),
                ClosedReportMessageIds = new List<int>(),
            };

            return history;
        }
    }
}
