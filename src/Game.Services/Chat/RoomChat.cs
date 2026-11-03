using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

namespace Game.Services
{
    internal sealed class RoomChat
    {
        private readonly Queue<ChatMessage> _keptMessages = new Queue<ChatMessage>();
        private readonly int _roomId;

        private int _lastMessageId;

        internal RoomChat(int roomId)
        {
            _roomId = roomId;
        }

        internal ChatMessage Append(LiveRoomMember author, ChatDraft messageDraft, int messagesKeptPerRoom)
        {
            _lastMessageId++;
            var appendedMessage = new ChatMessage
            {
                RoomId = _roomId,
                MessageId = _lastMessageId,
                SenderMemberId = author.MemberId,
                SenderDisplayName = author.DisplayName,
                SenderIsGuest = author.IsGuest,
                Text = messageDraft.Text,
                SentAtUtc = messageDraft.SentAtUtc,
            };
            _keptMessages.Enqueue(appendedMessage);
            while (_keptMessages.Count > messagesKeptPerRoom)
            {
                _keptMessages.Dequeue();
            }

            return appendedMessage;
        }

        internal IReadOnlyList<ChatMessage> ListMessages()
        {
            List<ChatMessage> keptMessagesCopy = _keptMessages.ToList();

            return keptMessagesCopy;
        }
    }
}
