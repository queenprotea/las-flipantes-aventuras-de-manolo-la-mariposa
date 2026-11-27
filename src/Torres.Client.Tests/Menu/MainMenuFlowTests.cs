using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class MainMenuFlowTests
    {
        private const string GetPendingInvitationsCall = nameof(RecordingGameSessionConnection.GetPendingInvitationsAsync);
        private const int OneInvitation = 1;
        private const int NoInvitations = 0;

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly MainMenuFlow _mainMenuFlow;

        public MainMenuFlowTests()
        {
            _gameSessionConnection.GetPendingInvitationsReply = Task.FromResult(new PendingInvitationsResponse { Invitations = new List<RoomInvitation> { new RoomInvitation { RoomId = SampleClients.RoomId } } });
            _mainMenuFlow = new MainMenuFlow(_gameSessionConnection, _playerSession);
        }

        [Fact]
        public void Test_Open_WithAnAccount_AsksForTheInvitations()
        {
            _mainMenuFlow.Open();

            Assert.Contains(GetPendingInvitationsCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_Open_AsAGuest_DoesNotAskForTheInvitations()
        {
            _playerSession.StartAsGuest(SampleClients.GuestAlias);

            _mainMenuFlow.Open();

            Assert.DoesNotContain(GetPendingInvitationsCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_CountInvitations_AfterTheInvitationsArrive_CountsThem()
        {
            _mainMenuFlow.Open();
            _mainMenuFlow.Follow();

            int invitationCount = _mainMenuFlow.CountInvitations();

            Assert.Equal(OneInvitation, invitationCount);
        }

        [Fact]
        public void Test_CountInvitations_AsAGuest_ReturnsZero()
        {
            _playerSession.StartAsGuest(SampleClients.GuestAlias);

            int invitationCount = _mainMenuFlow.CountInvitations();

            Assert.Equal(NoInvitations, invitationCount);
        }

        [Fact]
        public void Test_Follow_WhenTheInvitationsFail_KeepsTheInboxEmpty()
        {
            _gameSessionConnection.GetPendingInvitationsReply = SampleClients.CreateFailedCallAsync<PendingInvitationsResponse>();
            _mainMenuFlow.Open();

            _mainMenuFlow.Follow();

            Assert.Empty(_playerSession.Invitations.Invitations);
        }

        [Fact]
        public void Test_CanUseAccountFeatures_AsAGuest_ReturnsFalse()
        {
            _playerSession.StartAsGuest(SampleClients.GuestAlias);

            bool canUseAccountFeatures = _mainMenuFlow.CanUseAccountFeatures;

            Assert.False(canUseAccountFeatures);
        }

        [Fact]
        public void Test_InvitationsRevision_AfterTheInvitationsArrive_ChangesTheRevision()
        {
            int revisionBefore = _mainMenuFlow.InvitationsRevision;
            _mainMenuFlow.Open();

            _mainMenuFlow.Follow();

            int revisionAfter = _mainMenuFlow.InvitationsRevision;
            Assert.NotEqual(revisionBefore, revisionAfter);
        }
    }
}
