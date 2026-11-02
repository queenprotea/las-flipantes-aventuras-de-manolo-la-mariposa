using System.Collections.Generic;

using Game.Contracts;

namespace Torres.Client.Session
{
    internal sealed class RoomChatLog
    {
        private readonly List<ChatMessage> _messages = new List<ChatMessage>();

        private int _messagesKeptPerRoom;

        internal int? RoomId { get; private set; }

        internal bool IsLoaded { get; private set; }

        internal int MessageMaxLength { get; private set; }

        internal int Revision { get; private set; }

        internal IReadOnlyList<ChatMessage> Messages => _messages;

        internal void Follow(int roomId)
        {
            if (RoomId != roomId)
            {
                Clear();
                RoomId = roomId;
            }
        }

        internal void Load(int roomId, ChatHistoryResponse history)
        {
            bool isForFollowedRoom = RoomId == roomId;
            if (!isForFollowedRoom)
            {
                return;
            }

            foreach (ChatMessage message in history.Messages)
            {
                Insert(message);
            }

            MessageMaxLength = history.MessageMaxLength;
            _messagesKeptPerRoom = history.MessagesKeptPerRoom;
            IsLoaded = true;
            DiscardBeyondWindow();
            Revision++;
        }

        internal void Add(ChatMessage message)
        {
            bool isForFollowedRoom = RoomId == message.RoomId;
            if (!isForFollowedRoom)
            {
                return;
            }

            bool wasInserted = Insert(message);
            if (wasInserted)
            {
                DiscardBeyondWindow();
                Revision++;
            }
        }

        internal void Clear()
        {
            _messages.Clear();
            _messagesKeptPerRoom = 0;
            RoomId = null;
            IsLoaded = false;
            MessageMaxLength = 0;
            Revision++;
        }

        private bool Insert(ChatMessage message)
        {
            int insertIndex = _messages.Count;
            while ((insertIndex > 0) && (_messages[insertIndex - 1].MessageId > message.MessageId))
            {
                insertIndex--;
            }

            bool isRepeated = (insertIndex > 0) && (_messages[insertIndex - 1].MessageId == message.MessageId);
            bool wasInserted = !isRepeated;
            if (wasInserted)
            {
                _messages.Insert(insertIndex, message);
            }

            return wasInserted;
        }

        private void DiscardBeyondWindow()
        {
            int excessCount = _messagesKeptPerRoom > 0 ? _messages.Count - _messagesKeptPerRoom : 0;
            if (excessCount > 0)
            {
                _messages.RemoveRange(0, excessCount);
            }
        }
    }
}
