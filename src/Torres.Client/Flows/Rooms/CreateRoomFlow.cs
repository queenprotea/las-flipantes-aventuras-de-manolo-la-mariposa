using System;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class CreateRoomFlow
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<CreateRoomResult> _creationRequest = new PendingRequest<CreateRoomResult>();

        internal CreateRoomFlow(GameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal bool IsCreating => _creationRequest.IsWaiting;

        internal ScreenId? FindRedirect()
        {
            ScreenId? redirect = null;
            if (!_playerSession.IsLoggedIn || !_gameSessionConnection.IsSessionOpen)
            {
                redirect = ScreenId.Login;
            }
            else if (_playerSession.CurrentRoom is not null)
            {
                redirect = ScreenId.Room;
            }

            return redirect;
        }

        internal void StartCreation(bool isPublic)
        {
            if (!_creationRequest.IsWaiting)
            {
                RoomVisibility visibility = isPublic ? RoomVisibility.Public : RoomVisibility.Private;
                _creationRequest.Start(_gameSessionConnection.CreateRoomAsync(visibility));
            }
        }

        internal ScreenOutcome? TakeOutcome()
        {
            ScreenOutcome? outcome = null;
            FinishedRequest<CreateRoomResult>? finishedCreation = _creationRequest.TakeFinished();
            if ((finishedCreation is not null) && finishedCreation.IsSuccessful && (finishedCreation.Reply is CreateRoomResult creation))
            {
                outcome = ToOutcome(creation);
            }
            else if (finishedCreation is not null)
            {
                outcome = new ScreenOutcome(Notice.CreateServerError(), null);
            }

            return outcome;
        }

        private ScreenOutcome ToOutcome(CreateRoomResult creation)
        {
            ScreenOutcome outcome = creation.Status switch
            {
                CreateRoomStatus.Created => EnterRoom(creation.Room),
                CreateRoomStatus.AlreadyInRoom => new ScreenOutcome(Notice.CreateTitle(TextKeys.Rooms.AlreadyInRoomError), null),
                CreateRoomStatus.Banned => new ScreenOutcome(Notice.CreateBan(creation.BanEndsAtUtc), null),
                CreateRoomStatus.NotIdentified => new ScreenOutcome(null, ScreenId.Login),
                CreateRoomStatus.InvalidVisibility or CreateRoomStatus.DatabaseUnavailable => new ScreenOutcome(Notice.CreateServerError(), null),
                _ => throw new ArgumentOutOfRangeException(nameof(creation), creation.Status, null),
            };

            return outcome;
        }

        private ScreenOutcome EnterRoom(Room? createdRoom)
        {
            var outcome = new ScreenOutcome(Notice.CreateServerError(), null);
            if (createdRoom is not null)
            {
                _playerSession.EnterRoom(createdRoom);
                outcome = new ScreenOutcome(null, ScreenId.Room);
            }

            return outcome;
        }
    }
}
