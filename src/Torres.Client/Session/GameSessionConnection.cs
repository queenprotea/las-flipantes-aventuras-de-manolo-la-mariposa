using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

namespace Torres.Client.Session
{
    internal sealed class GameSessionConnection
    {
        private const string ClosedConnectionMessage = "The game session is not open.";

        private static readonly TimeSpan _closeWaitTime = TimeSpan.FromSeconds(2);

        private readonly GameSessionNoticeQueue _noticeQueue = new GameSessionNoticeQueue();
        private readonly DuplexChannelFactory<IGameSessionService> _channelFactory;

        private IGameSessionService? _channel;
        private Task<HeartbeatResponse>? _heartbeat;
        private TimeSpan _heartbeatPeriod;
        private TimeSpan _sinceLastHeartbeat;

        internal GameSessionConnection(ClientSettings clientSettings)
        {
            ArgumentNullException.ThrowIfNull(clientSettings);

            _channelFactory = new DuplexChannelFactory<IGameSessionService>(
                new InstanceContext(_noticeQueue),
                new NetTcpBinding(SecurityMode.None),
                clientSettings.CreateEndpointAddress(ServiceEndpointPaths.GameSession));
        }

        internal bool IsOpen { get; private set; }

        internal bool WasLost { get; private set; }

        internal Task<OpenSessionResult> OpenAsync(string sessionToken)
        {
            ArgumentNullException.ThrowIfNull(sessionToken);

            Abandon();
            IGameSessionService openingChannel = _channelFactory.CreateChannel();
            _channel = openingChannel;
            Task<OpenSessionResult> opening = new SessionCall<string, OpenSessionResult>(openingChannel.OpenSession, sessionToken).StartAsync();

            return opening;
        }

        internal void Start(OpenSessionResult opening)
        {
            ArgumentNullException.ThrowIfNull(opening);

            _heartbeatPeriod = TimeSpan.FromSeconds(opening.HeartbeatPeriodSeconds);
            _sinceLastHeartbeat = TimeSpan.Zero;
            IsOpen = true;
            WasLost = false;
        }

        internal void Update(GameTime gameTime)
        {
            if (!IsOpen || (_channel is null))
            {
                return;
            }

            _sinceLastHeartbeat += gameTime.ElapsedGameTime;
            FollowHeartbeat();
            bool isHeartbeatDue = (_heartbeat is null) && (_sinceLastHeartbeat >= _heartbeatPeriod);
            if (isHeartbeatDue)
            {
                _sinceLastHeartbeat = TimeSpan.Zero;
                _heartbeat = Task.Run(_channel.IsAlive);
            }

            if (_channel is ICommunicationObject { State: CommunicationState.Faulted or CommunicationState.Closed })
            {
                WasLost = true;
            }
        }

        internal RoomChange? TakeRoomChange()
        {
            RoomChange? roomChange = _noticeQueue.TakeRoomChange();

            return roomChange;
        }

        internal SessionTerminationReason? TakeTerminationReason()
        {
            SessionTerminationReason? terminationReason = _noticeQueue.TakeTerminationReason();

            return terminationReason;
        }

        internal Task<CreateRoomResult> CreateRoomAsync(RoomVisibility visibility)
        {
            Task<CreateRoomResult> creation = RequireChannel().CreateRoomAsync(visibility);

            return creation;
        }

        internal Task<RoomListResponse> GetRoomsAsync()
        {
            Task<RoomListResponse> listing = Task.Run(RequireChannel().GetRooms);

            return listing;
        }

        internal Task<RoomListResponse> FindRoomAsync(string roomCode)
        {
            Task<RoomListResponse> search = new SessionCall<string, RoomListResponse>(RequireChannel().FindRoom, roomCode).StartAsync();

            return search;
        }

        internal Task<JoinRoomResult> JoinRoomAsync(int roomId)
        {
            Task<JoinRoomResult> joining = RequireChannel().JoinRoomAsync(roomId);

            return joining;
        }

        internal Task<LeaveRoomResult> LeaveRoomAsync()
        {
            Task<LeaveRoomResult> leaving = RequireChannel().LeaveRoomAsync();

            return leaving;
        }

        internal Task<KickMemberResult> KickMemberAsync(int targetMemberId)
        {
            Task<KickMemberResult> kick = new SessionCall<int, KickMemberResult>(RequireChannel().KickMember, targetMemberId).StartAsync();

            return kick;
        }

        internal void Close()
        {
            if (IsOpen && (_channel is not null))
            {
                Task<CloseSessionResult> closing = _channel.CloseSessionAsync();
                Task.WaitAny(new Task[] { closing }, _closeWaitTime);
            }

            Abandon();
            _channelFactory.Abort();
        }

        internal void Abandon()
        {
            if (_channel is ICommunicationObject abandonedChannel)
            {
                abandonedChannel.Abort();
            }

            _channel = null;
            _heartbeat = null;
            IsOpen = false;
            WasLost = false;
            _noticeQueue.Clear();
        }

        private void FollowHeartbeat()
        {
            if ((_heartbeat is null) || !_heartbeat.IsCompleted)
            {
                return;
            }

            WasLost = !_heartbeat.IsCompletedSuccessfully;
            _heartbeat = null;
        }

        private IGameSessionService RequireChannel()
        {
            if (!IsOpen || (_channel is null))
            {
                throw new InvalidOperationException(ClosedConnectionMessage);
            }

            return _channel;
        }
    }
}
