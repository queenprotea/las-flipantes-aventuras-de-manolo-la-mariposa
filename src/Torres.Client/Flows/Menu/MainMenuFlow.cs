using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class MainMenuFlow
    {
        private readonly IGameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<PendingInvitationsResponse> _invitationsRequest = new PendingRequest<PendingInvitationsResponse>();

        internal MainMenuFlow(IGameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal bool CanUseAccountFeatures => !_playerSession.IsGuest;

        internal int InvitationsRevision => _playerSession.Invitations.Revision;

        internal void Open()
        {
            bool canReadInvitations = _playerSession.HasAccount && _gameSessionConnection.IsSessionOpen && !_invitationsRequest.IsWaiting;
            if (canReadInvitations)
            {
                _invitationsRequest.Start(_gameSessionConnection.GetPendingInvitationsAsync());
            }
        }

        internal int CountInvitations()
        {
            int invitationCount = _playerSession.HasAccount ? _playerSession.Invitations.Invitations.Count : 0;

            return invitationCount;
        }

        internal void Follow()
        {
            FinishedRequest<PendingInvitationsResponse>? finishedRequest = _invitationsRequest.TakeFinished();
            if ((finishedRequest is null) || !_playerSession.HasAccount)
            {
                return;
            }

            if (finishedRequest.IsSuccessful && (finishedRequest.Reply is PendingInvitationsResponse pendingInvitations))
            {
                _playerSession.Invitations.Load(pendingInvitations);
            }
        }
    }
}
