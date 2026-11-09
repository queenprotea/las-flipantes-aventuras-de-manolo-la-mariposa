using System;
using System.Threading.Tasks;

using Game.Contracts;

namespace Torres.Client.Session
{
    internal sealed class OwnProfileReader
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;

        private Task<ProfileResponse>? _profileRequest;
        private bool _wasProfileRead;

        internal OwnProfileReader(GameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(playerSession);

            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal void Update()
        {
            if (!_playerSession.HasAccount || !_gameSessionConnection.IsSessionOpen)
            {
                _profileRequest = null;
                _wasProfileRead = false;
                return;
            }

            if (!_wasProfileRead && (_profileRequest is null))
            {
                _profileRequest = _gameSessionConnection.GetProfileAsync();
            }

            FollowProfileRequest();
        }

        private void FollowProfileRequest()
        {
            if ((_profileRequest is null) || !_profileRequest.IsCompleted)
            {
                return;
            }

            if (_profileRequest.IsCompletedSuccessfully)
            {
                _playerSession.ApplyProfile(_profileRequest.Result.Username, _profileRequest.Result.AvatarReference);
            }

            _profileRequest = null;
            _wasProfileRead = true;
        }
    }
}
