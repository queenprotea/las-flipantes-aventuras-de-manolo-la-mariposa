using System;
using System.Collections.Generic;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class RoomsFlow
    {
        private readonly IGameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<RoomListResponse> _listRequest = new PendingRequest<RoomListResponse>();
        private readonly PendingRequest<JoinRoomResult> _joinRequest = new PendingRequest<JoinRoomResult>();

        internal RoomsFlow(IGameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal bool IsShowingSearch { get; private set; }

        internal static ScreenId FindScreenToCreateRoom()
        {
            ScreenId nextScreen = ScreenId.CreateRoom;

            return nextScreen;
        }

        internal static ScreenId FindScreenToGoBack()
        {
            ScreenId nextScreen = ScreenId.MainMenu;

            return nextScreen;
        }

        internal ScreenId? Open()
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
            else
            {
                RequestRooms();
            }

            return redirect;
        }

        internal void RequestRooms()
        {
            IsShowingSearch = false;
            _listRequest.Forget();
            _listRequest.Start(_gameSessionConnection.GetRoomsAsync());
        }

        internal void Search(string typedRoomCode)
        {
            string roomCode = typedRoomCode.Trim();
            if (roomCode.Length == 0)
            {
                RequestRooms();
            }
            else
            {
                IsShowingSearch = true;
                _listRequest.Forget();
                _listRequest.Start(_gameSessionConnection.FindRoomAsync(roomCode));
            }
        }

        internal bool StartJoin(int roomId)
        {
            bool isStarted = !_joinRequest.IsWaiting;
            if (isStarted)
            {
                _joinRequest.Start(_gameSessionConnection.JoinRoomAsync(roomId));
            }

            return isStarted;
        }

        internal RoomsOutcome? TakeOutcome()
        {
            ScreenOutcome? joinOutcome = FollowJoinRequest();
            FinishedRequest<RoomListResponse>? finishedList = _listRequest.TakeFinished();
            List<RoomSummary>? rooms = ReadRooms(finishedList);
            Notice? notice = joinOutcome?.Notice;
            if ((finishedList is not null) && (rooms is null))
            {
                notice = Notice.CreateServerError();
            }

            ScreenId? nextScreen = joinOutcome?.NextScreen;
            RoomsOutcome? outcome = null;
            if ((rooms is not null) || (notice is not null) || (nextScreen is not null))
            {
                outcome = new RoomsOutcome(rooms, notice, nextScreen);
            }

            return outcome;
        }

        private static List<RoomSummary>? ReadRooms(FinishedRequest<RoomListResponse>? finishedList)
        {
            List<RoomSummary>? rooms = null;
            if ((finishedList is not null) && finishedList.IsSuccessful && (finishedList.Reply is RoomListResponse roomList))
            {
                rooms = roomList.Rooms;
            }

            return rooms;
        }

        private ScreenOutcome? FollowJoinRequest()
        {
            ScreenOutcome? outcome = null;
            FinishedRequest<JoinRoomResult>? finishedJoin = _joinRequest.TakeFinished();
            if ((finishedJoin is not null) && finishedJoin.IsSuccessful && (finishedJoin.Reply is JoinRoomResult joining))
            {
                outcome = ToOutcome(joining);
            }
            else if (finishedJoin is not null)
            {
                outcome = new ScreenOutcome(Notice.CreateServerError(), null);
            }

            return outcome;
        }

        private ScreenOutcome ToOutcome(JoinRoomResult joining)
        {
            ScreenOutcome outcome = joining.Status switch
            {
                JoinRoomStatus.Joined => EnterRoom(joining.Room),
                JoinRoomStatus.RoomFull => Refuse(TextKeys.Alerts.RoomFullTitle, TextKeys.Alerts.RoomFullBody),
                JoinRoomStatus.RoomNotFound => Refuse(TextKeys.Alerts.RoomNotFoundTitle, TextKeys.Alerts.RoomNotFoundBody),
                JoinRoomStatus.RoomInMatch => new ScreenOutcome(Notice.CreateTitle(TextKeys.Rooms.RoomInMatchError), null),
                JoinRoomStatus.AlreadyInRoom => new ScreenOutcome(Notice.CreateTitle(TextKeys.Rooms.AlreadyInRoomError), null),
                JoinRoomStatus.Banned => new ScreenOutcome(Notice.CreateBan(joining.BanEndsAtUtc), null),
                JoinRoomStatus.NotIdentified => new ScreenOutcome(null, ScreenId.Login),
                JoinRoomStatus.DatabaseUnavailable => new ScreenOutcome(Notice.CreateServerError(), null),
                _ => throw new ArgumentOutOfRangeException(nameof(joining), joining.Status, null),
            };

            return outcome;
        }

        private ScreenOutcome Refuse(string titleKey, string bodyKey)
        {
            RequestRooms();
            var refusal = new ScreenOutcome(Notice.CreateMessage(titleKey, bodyKey, null), null);

            return refusal;
        }

        private ScreenOutcome EnterRoom(Room? joinedRoom)
        {
            var outcome = new ScreenOutcome(Notice.CreateServerError(), null);
            if (joinedRoom is not null)
            {
                _playerSession.EnterRoom(joinedRoom);
                outcome = new ScreenOutcome(null, ScreenId.Room);
            }

            return outcome;
        }
    }
}
