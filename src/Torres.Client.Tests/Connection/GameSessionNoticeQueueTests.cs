using Game.Contracts;

using Torres.Client.Connection;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class GameSessionNoticeQueueTests
    {
        private readonly GameSessionNoticeQueue _noticeQueue = new GameSessionNoticeQueue();
        private readonly Room _room = SampleClients.CreateRoom();
        private readonly MatchView _matchView = new MatchView { RoomCode = SampleClients.RoomCode };
        private readonly ChatMessage _firstMessage = new ChatMessage();
        private readonly ChatMessage _secondMessage = new ChatMessage();

        [Fact]
        public void Test_TakeRoomChange_AfterARoomChange_ReturnsTheChange()
        {
            _noticeQueue.ReceiveRoomStateChanged(RoomChangeReason.MemberJoined, _room);

            RoomChange? roomChange = _noticeQueue.TakeRoomChange();

            Assert.Equal(new RoomChange(RoomChangeReason.MemberJoined, _room), roomChange);
        }

        [Fact]
        public void Test_TakeRoomChange_WithoutChanges_ReturnsNull()
        {
            RoomChange? roomChange = _noticeQueue.TakeRoomChange();

            Assert.Null(roomChange);
        }

        [Fact]
        public void Test_TakeTerminationReason_AfterTheSessionWasTerminated_ReturnsTheReason()
        {
            _noticeQueue.ReceiveSessionTerminated(SessionTerminationReason.AccountSignedInElsewhere);

            SessionTerminationReason? terminationReason = _noticeQueue.TakeTerminationReason();

            Assert.Equal(SessionTerminationReason.AccountSignedInElsewhere, terminationReason);
        }

        [Fact]
        public void Test_TakeTerminationReason_WithoutTermination_ReturnsNull()
        {
            SessionTerminationReason? terminationReason = _noticeQueue.TakeTerminationReason();

            Assert.Null(terminationReason);
        }

        [Fact]
        public void Test_TakeRoomInvitation_AfterAnInvitation_ReturnsTheInvitation()
        {
            var invitation = new RoomInvitation();
            _noticeQueue.ReceiveRoomInvitation(invitation);

            RoomInvitation? receivedInvitation = _noticeQueue.TakeRoomInvitation();

            Assert.Same(invitation, receivedInvitation);
        }

        [Fact]
        public void Test_TakeRoomInvitation_WithoutInvitations_ReturnsNull()
        {
            RoomInvitation? receivedInvitation = _noticeQueue.TakeRoomInvitation();

            Assert.Null(receivedInvitation);
        }

        [Fact]
        public void Test_TakeChatMessage_AfterTwoMessages_ReturnsTheFirstOne()
        {
            _noticeQueue.ReceiveChatMessage(_firstMessage);
            _noticeQueue.ReceiveChatMessage(_secondMessage);

            ChatMessage? chatMessage = _noticeQueue.TakeChatMessage();

            Assert.Same(_firstMessage, chatMessage);
        }

        [Fact]
        public void Test_TakeChatMessage_WithoutMessages_ReturnsNull()
        {
            ChatMessage? chatMessage = _noticeQueue.TakeChatMessage();

            Assert.Null(chatMessage);
        }

        [Fact]
        public void Test_TakeMatchChange_AfterTheMatchStarted_ReturnsAStartedChange()
        {
            _noticeQueue.ReceiveMatchStarted(_matchView);

            MatchChange? matchChange = _noticeQueue.TakeMatchChange();

            Assert.Equal(MatchChangeKind.Started, matchChange?.Kind);
        }

        [Fact]
        public void Test_TakeMatchChange_AfterTheMatchStarted_CarriesTheMatchView()
        {
            _noticeQueue.ReceiveMatchStarted(_matchView);

            MatchChange? matchChange = _noticeQueue.TakeMatchChange();

            Assert.Same(_matchView, matchChange?.MatchView);
        }

        [Fact]
        public void Test_TakeMatchChange_AfterTheMatchChanged_ReturnsAStateChange()
        {
            _noticeQueue.ReceiveMatchStateChanged(_matchView);

            MatchChange? matchChange = _noticeQueue.TakeMatchChange();

            Assert.Equal(MatchChangeKind.StateChanged, matchChange?.Kind);
        }

        [Fact]
        public void Test_TakeMatchChange_WithoutChanges_ReturnsNull()
        {
            MatchChange? matchChange = _noticeQueue.TakeMatchChange();

            Assert.Null(matchChange);
        }

        [Fact]
        public void Test_Clear_WithPendingNotices_DiscardsThem()
        {
            _noticeQueue.ReceiveChatMessage(_firstMessage);

            _noticeQueue.Clear();

            ChatMessage? chatMessage = _noticeQueue.TakeChatMessage();
            Assert.Null(chatMessage);
        }
    }
}
