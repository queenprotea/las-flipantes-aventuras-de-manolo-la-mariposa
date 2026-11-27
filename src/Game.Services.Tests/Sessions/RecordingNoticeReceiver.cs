using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Game.Contracts;

namespace Game.Services.Tests
{
    internal sealed class RecordingNoticeReceiver : IGameSessionNoticeReceiver
    {
        private readonly Lock _noticeLock = new Lock();
        private readonly TaskCompletionSource<ChatMessage> _firstChatMessage = new TaskCompletionSource<ChatMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<RoomInvitation> _firstInvitation = new TaskCompletionSource<RoomInvitation>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Dictionary<RoomChangeReason, TaskCompletionSource<Room>> _roomChangesByReason = new Dictionary<RoomChangeReason, TaskCompletionSource<Room>>();

        internal Task<ChatMessage> FirstChatMessage => _firstChatMessage.Task;

        internal Task<RoomInvitation> FirstInvitation => _firstInvitation.Task;

        public void ReceiveSessionTerminated(SessionTerminationReason reason)
        {
        }

        public void ReceiveRoomStateChanged(RoomChangeReason reason, Room room)
        {
            FindRoomChange(reason).TrySetResult(room);
        }

        public void ReceiveRoomInvitation(RoomInvitation invitation)
        {
            _firstInvitation.TrySetResult(invitation);
        }

        public void ReceiveChatMessage(ChatMessage message)
        {
            _firstChatMessage.TrySetResult(message);
        }

        public void ReceiveMatchStarted(MatchView matchView)
        {
        }

        public void ReceiveMatchStateChanged(MatchView matchView)
        {
        }

        internal Task<Room> WaitForRoomChangeAsync(RoomChangeReason reason)
        {
            Task<Room> roomChange = FindRoomChange(reason).Task;

            return roomChange;
        }

        private TaskCompletionSource<Room> FindRoomChange(RoomChangeReason reason)
        {
            TaskCompletionSource<Room>? roomChange;
            lock (_noticeLock)
            {
                if (!_roomChangesByReason.TryGetValue(reason, out roomChange))
                {
                    roomChange = new TaskCompletionSource<Room>(TaskCreationOptions.RunContinuationsAsynchronously);
                    _roomChangesByReason.Add(reason, roomChange);
                }
            }

            return roomChange;
        }
    }
}
