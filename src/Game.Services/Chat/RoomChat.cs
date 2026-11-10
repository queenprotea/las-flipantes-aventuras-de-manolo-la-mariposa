using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

namespace Game.Services
{
    internal sealed class RoomChat
    {
        private readonly Queue<RoomChatEntry> _keptEntries = new Queue<RoomChatEntry>();
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
            _keptEntries.Enqueue(new RoomChatEntry(appendedMessage, author.OwnerId));
            while (_keptEntries.Count > messagesKeptPerRoom)
            {
                _keptEntries.Dequeue();
            }

            return appendedMessage;
        }

        internal IReadOnlyList<ChatMessage> ListMessages()
        {
            List<ChatMessage> keptMessagesCopy = _keptEntries.Select(keptEntry => keptEntry.Message).ToList();

            return keptMessagesCopy;
        }

        internal RoomChatEntry? FindEntry(int messageId)
        {
            RoomChatEntry? foundEntry = _keptEntries.FirstOrDefault(keptEntry => keptEntry.Message.MessageId == messageId);

            return foundEntry;
        }
    }
}
