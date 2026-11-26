using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class OwnProfileFlow
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<ProfileResponse> _profileRequest = new PendingRequest<ProfileResponse>();

        private bool _wasProfileRead;

        internal OwnProfileFlow(GameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal void Follow()
        {
            bool canReadProfile = _playerSession.HasAccount && _gameSessionConnection.IsSessionOpen;
            if (!canReadProfile)
            {
                _profileRequest.Forget();
                _wasProfileRead = false;
            }
            else if (!_wasProfileRead && !_profileRequest.IsWaiting)
            {
                _profileRequest.Start(_gameSessionConnection.GetProfileAsync());
            }

            FollowProfileRequest();
        }

        private void FollowProfileRequest()
        {
            FinishedRequest<ProfileResponse>? finishedProfile = _profileRequest.TakeFinished();
            if ((finishedProfile is not null) && finishedProfile.IsSuccessful && (finishedProfile.Reply is ProfileResponse profile))
            {
                _playerSession.ApplyProfile(profile.Username, profile.AvatarReference);
            }

            _wasProfileRead |= finishedProfile is not null;
        }
    }
}
