using System;
using System.Collections.Concurrent;
using System.ServiceModel;

using Game.Contracts;

namespace Torres.Client.Session
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    internal sealed class GameSessionNoticeQueue : IGameSessionServiceCallback
    {
        private readonly ConcurrentQueue<RoomChange> _roomChanges = new ConcurrentQueue<RoomChange>();
        private readonly ConcurrentQueue<SessionTerminationReason> _terminationReasons = new ConcurrentQueue<SessionTerminationReason>();
        private readonly ConcurrentQueue<ChatMessage> _chatMessages = new ConcurrentQueue<ChatMessage>();
        private readonly ConcurrentQueue<MatchChange> _matchChanges = new ConcurrentQueue<MatchChange>();
        private readonly ConcurrentQueue<RoomInvitation> _roomInvitations = new ConcurrentQueue<RoomInvitation>();

        public void ReceiveRoomStateChanged(RoomChangeReason reason, Room room)
        {
            _roomChanges.Enqueue(new RoomChange(reason, room));
        }

        public void ReceiveSessionTerminated(SessionTerminationReason reason)
        {
            _terminationReasons.Enqueue(reason);
        }

        public void ReceiveRoomInvitation(RoomInvitation invitation)
        {
            _roomInvitations.Enqueue(invitation);
        }

        public void ReceiveChatMessage(ChatMessage message)
        {
            _chatMessages.Enqueue(message);
        }

        public void ReceiveMatchStarted(MatchView matchView)
        {
            _matchChanges.Enqueue(new MatchChange(MatchChangeKind.Started, matchView, DateTime.UtcNow));
        }

        public void ReceiveMatchStateChanged(MatchView matchView)
        {
            _matchChanges.Enqueue(new MatchChange(MatchChangeKind.StateChanged, matchView, DateTime.UtcNow));
        }

        internal RoomChange? TakeRoomChange()
        {
            RoomChange? roomChange = _roomChanges.TryDequeue(out RoomChange? pendingChange) ? pendingChange : null;

            return roomChange;
        }

        internal SessionTerminationReason? TakeTerminationReason()
        {
            SessionTerminationReason? terminationReason = _terminationReasons.TryDequeue(out SessionTerminationReason pendingReason)
                ? pendingReason
                : null;

            return terminationReason;
        }

        internal ChatMessage? TakeChatMessage()
        {
            ChatMessage? chatMessage = _chatMessages.TryDequeue(out ChatMessage? pendingMessage) ? pendingMessage : null;

            return chatMessage;
        }

        internal MatchChange? TakeMatchChange()
        {
            MatchChange? matchChange = _matchChanges.TryDequeue(out MatchChange? pendingChange) ? pendingChange : null;

            return matchChange;
        }

        internal RoomInvitation? TakeRoomInvitation()
        {
            RoomInvitation? roomInvitation = _roomInvitations.TryDequeue(out RoomInvitation? pendingInvitation) ? pendingInvitation : null;

            return roomInvitation;
        }

        internal void Clear()
        {
            _roomChanges.Clear();
            _terminationReasons.Clear();
            _chatMessages.Clear();
            _matchChanges.Clear();
            _roomInvitations.Clear();
        }
    }
}
