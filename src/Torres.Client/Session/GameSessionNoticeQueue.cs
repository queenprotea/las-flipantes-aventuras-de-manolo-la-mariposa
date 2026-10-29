using System.Collections.Concurrent;
using System.ServiceModel;

using Game.Contracts;

namespace Torres.Client.Session
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    internal sealed class GameSessionNoticeQueue : IGameSessionNoticeReceiver
    {
        private readonly ConcurrentQueue<RoomChange> _roomChanges = new ConcurrentQueue<RoomChange>();
        private readonly ConcurrentQueue<SessionTerminationReason> _terminationReasons = new ConcurrentQueue<SessionTerminationReason>();

        public void ReceiveRoomStateChanged(RoomChangeReason reason, Room room)
        {
            _roomChanges.Enqueue(new RoomChange(reason, room));
        }

        public void ReceiveSessionTerminated(SessionTerminationReason reason)
        {
            _terminationReasons.Enqueue(reason);
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

        internal void Clear()
        {
            _roomChanges.Clear();
            _terminationReasons.Clear();
        }
    }
}
