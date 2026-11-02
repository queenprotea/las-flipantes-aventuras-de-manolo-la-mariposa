using System;

namespace Game.Services
{
    public sealed class ChatLimits
    {
        public ChatLimits(int messageMaxLength, int messagesKeptPerRoom)
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
