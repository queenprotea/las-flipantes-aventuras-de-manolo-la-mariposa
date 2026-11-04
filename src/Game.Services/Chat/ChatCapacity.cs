using System;

namespace Game.Services
{
    public sealed class ChatCapacity
    {
        public ChatCapacity(int messageMaxLength, int messagesKeptPerRoom)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(messageMaxLength);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(messagesKeptPerRoom);

            MessageMaxLength = messageMaxLength;
            MessagesKeptPerRoom = messagesKeptPerRoom;
        }

        public int MessageMaxLength { get; }

        public int MessagesKeptPerRoom { get; }
    }
}
