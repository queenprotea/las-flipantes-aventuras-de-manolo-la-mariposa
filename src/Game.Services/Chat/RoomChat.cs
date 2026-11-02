using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

namespace Game.Services
{
    internal sealed class RoomChat
    {
        private readonly Queue<ChatMessage> _messages = new Queue<ChatMessage>();
        private readonly int _roomId;

        private int _lastMessageId;

        internal RoomChat(int roomId)
        {
            _roomId = roomId;
        }

        internal ChatMessage Append(LiveRoomMember author, ChatDraft draft, int messagesKept)
        {
            _lastMessageId++;
            var message = new ChatMessage
            {
                RoomId = _roomId,
                MessageId = _lastMessageId,
                SenderMemberId = author.MemberId,
                SenderDisplayName = author.DisplayName,
                SenderIsGuest = author.IsGuest,
                Text = draft.Text,
                SentAtUtc = draft.SentAtUtc,
            };
            _messages.Enqueue(message);
            while (_messages.Count > messagesKept)
            {
                _messages.Dequeue();
            }

            return message;
        }

        internal IReadOnlyList<ChatMessage> ListMessages()
        {
            List<ChatMessage> messages = _messages.ToList();

            return messages;
        }
    }
}
