using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Contracts;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class SessionRegistryTests
    {
        private const string FirstSessionId = "session-a";
        private const string SecondSessionId = "session-b";
        private const string NewUsername = "ana_flores";
        private const string Greeting = "Hola";

        private static readonly TimeSpan _noticeWaitLimit = TimeSpan.FromSeconds(5);

        private readonly RecordingCallerChannel _callerChannel = new RecordingCallerChannel();
        private readonly List<int> _lostOwnerIds = new List<int>();
        private readonly List<int> _closedOwnerIds = new List<int>();
        private readonly SessionRegistry _sessionRegistry;

        public SessionRegistryTests()
        {
            _sessionRegistry = new SessionRegistry(_callerChannel);
            _sessionRegistry.SessionLost += (sender, e) => _lostOwnerIds.Add(e.OwnerId);
            _sessionRegistry.SessionClosed += (sender, e) => _closedOwnerIds.Add(e.OwnerId);
            _callerChannel.CallerSessionId = FirstSessionId;
        }

        [Fact]
        public void Test_Open_WithoutCallerSession_ThrowsInvalidOperationException()
        {
            _callerChannel.CallerSessionId = null;

            Assert.Throws<InvalidOperationException>(() => _sessionRegistry.Open(SamplePlayers.Ana));
        }

        [Fact]
        public void Test_FindOwner_OfAnOpenSession_ReturnsItsOwner()
        {
            _sessionRegistry.Open(SamplePlayers.Ana);

            SessionOwner? owner = _sessionRegistry.FindOwner(SamplePlayers.AnaId);

            Assert.Equal(SamplePlayers.Ana, owner);
        }

        [Fact]
        public void Test_FindOwner_WithoutSession_ReturnsNull()
        {
            SessionOwner? owner = _sessionRegistry.FindOwner(SamplePlayers.AnaId);

            Assert.Null(owner);
        }

        [Fact]
        public void Test_Open_FromAnotherChannel_TerminatesTheEarlierSession()
        {
            PlayerSession earlierSession = _sessionRegistry.Open(SamplePlayers.Ana);
            _callerChannel.CallerSessionId = SecondSessionId;

            _sessionRegistry.Open(SamplePlayers.Ana);

            Assert.True(earlierSession.WasTerminated);
        }

        [Fact]
        public void Test_Open_FromTheSameChannel_DoesNotTerminateTheEarlierSession()
        {
            PlayerSession earlierSession = _sessionRegistry.Open(SamplePlayers.Ana);

            _sessionRegistry.Open(SamplePlayers.Ana);

            Assert.False(earlierSession.WasTerminated);
        }

        [Fact]
        public void Test_Close_OfAnOpenSession_ReportsTheClosing()
        {
            PlayerSession session = _sessionRegistry.Open(SamplePlayers.Ana);

            _sessionRegistry.Close(session);

            Assert.Equal(new int[] { SamplePlayers.AnaId }, _closedOwnerIds);
        }

        [Fact]
        public void Test_DeclareLost_OfAnOpenSession_ReportsTheLoss()
        {
            PlayerSession session = _sessionRegistry.Open(SamplePlayers.Ana);

            _sessionRegistry.DeclareLost(session);

            Assert.Equal(new int[] { SamplePlayers.AnaId }, _lostOwnerIds);
        }

        [Fact]
        public void Test_DeclareLost_OfAReplacedSession_DoesNotReportTheLoss()
        {
            PlayerSession earlierSession = _sessionRegistry.Open(SamplePlayers.Ana);
            _callerChannel.CallerSessionId = SecondSessionId;
            _sessionRegistry.Open(SamplePlayers.Ana);

            _sessionRegistry.DeclareLost(earlierSession);

            Assert.Empty(_lostOwnerIds);
        }

        [Fact]
        public void Test_DeclareAbsent_OfAnOpenSession_ClosesIt()
        {
            _sessionRegistry.Open(SamplePlayers.Ana);

            _sessionRegistry.DeclareAbsent(SamplePlayers.AnaId);

            bool hasOpenSession = _sessionRegistry.HasOpenSession(SamplePlayers.AnaId);
            Assert.False(hasOpenSession);
        }

        [Fact]
        public void Test_DeclareAbsent_WithoutSession_ReportsNoLoss()
        {
            _sessionRegistry.DeclareAbsent(SamplePlayers.AnaId);

            Assert.Empty(_lostOwnerIds);
        }

        [Fact]
        public async Task Test_Deliver_ToAnOpenSession_SendsTheNotice()
        {
            var message = new ChatMessage { Text = Greeting };
            _sessionRegistry.Open(SamplePlayers.Ana);

            _sessionRegistry.Deliver(SamplePlayers.AnaId, new ChatMessageNotice(message));

            ChatMessage receivedMessage = await _callerChannel.FindReceiver(FirstSessionId).FirstChatMessage.WaitAsync(_noticeWaitLimit);
            Assert.Same(message, receivedMessage);
        }

        [Fact]
        public void Test_ApplyUsername_OfAnOpenSession_RenamesTheOwner()
        {
            PlayerSession session = _sessionRegistry.Open(SamplePlayers.Ana);

            session.ApplyUsername(NewUsername);

            Assert.Equal(NewUsername, session.Owner.DisplayName);
        }
    }
}
