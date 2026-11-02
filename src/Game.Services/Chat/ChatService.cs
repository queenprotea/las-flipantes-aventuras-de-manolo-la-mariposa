using System;
using System.Collections.Generic;

using Game.Contracts;

using log4net;

namespace Game.Services
{
    public sealed class ChatService : IChatService
    {
        private const string NotStartedMessage = "The chat limits were not set; Start must run before the chat is served.";
        private const string MessagePostedMessage = "Member {0} posted a chat message in room {1}.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(ChatService));

        private readonly RoomRegistry _roomRegistry;
        private readonly SessionRegistry _sessionRegistry;
        private readonly TimeProvider _timeProvider;

        private ChatLimits? _chatLimits;

        public ChatService(RoomRegistry roomRegistry, SessionRegistry sessionRegistry, TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(roomRegistry);
            ArgumentNullException.ThrowIfNull(sessionRegistry);
            ArgumentNullException.ThrowIfNull(timeProvider);

            _roomRegistry = roomRegistry;
            _sessionRegistry = sessionRegistry;
            _timeProvider = timeProvider;
        }

        public void Start(ChatLimits chatLimits)
        {
            ArgumentNullException.ThrowIfNull(chatLimits);

            _chatLimits = chatLimits;
        }

        public SendChatMessageResult SendChatMessage(string text)
        {
            ChatLimits chatLimits = RequireLimits();
            PlayerSession? caller = _sessionRegistry.FindCaller();
            string normalizedText = ChatTextNormalizer.Normalize(text);
            SendChatMessageResult sending;
            if (caller is null)
            {
                sending = SendChatMessageResult.NotIdentified;
            }
            else if (normalizedText.Length == 0)
            {
                sending = SendChatMessageResult.EmptyMessage;
            }
            else if (normalizedText.Length > chatLimits.MessageMaxLength)
            {
                sending = SendChatMessageResult.MessageTooLong;
            }
            else
            {
                sending = Post(caller.Owner.OwnerId, normalizedText, chatLimits);
            }

            return sending;
        }

        public ChatHistoryResponse GetChatHistory()
        {
            ChatLimits chatLimits = RequireLimits();
            PlayerSession? caller = _sessionRegistry.FindCaller();
            IReadOnlyList<ChatMessage>? messages = caller is null ? null : ReadHistoryOf(caller.Owner.OwnerId);
            var history = new ChatHistoryResponse
            {
                MessageMaxLength = chatLimits.MessageMaxLength,
                MessagesKeptPerRoom = chatLimits.MessagesKeptPerRoom,
            };
            if (messages is not null)
            {
                history.Messages.AddRange(messages);
            }

            return history;
        }

        private SendChatMessageResult Post(int authorOwnerId, string text, ChatLimits chatLimits)
        {
            var draft = new ChatDraft(text, _timeProvider.GetUtcNow().UtcDateTime);
            LiveRoom? room = _roomRegistry.FindRoomOf(authorOwnerId);
            ChatPosting? posting = room?.PostChatMessage(authorOwnerId, draft, chatLimits.MessagesKeptPerRoom);
            if ((room is null) || (posting is null))
            {
                return SendChatMessageResult.NotInRoom;
            }

            _log.DebugFormat(MessagePostedMessage, posting.Message.SenderMemberId, room.RoomId);
            var notice = new ChatMessageNotice(posting.Message);
            foreach (int recipientOwnerId in posting.RecipientOwnerIds)
            {
                _sessionRegistry.Deliver(recipientOwnerId, notice);
            }

            return SendChatMessageResult.Sent;
        }

        private IReadOnlyList<ChatMessage>? ReadHistoryOf(int ownerId)
        {
            IReadOnlyList<ChatMessage>? messages = _roomRegistry.FindRoomOf(ownerId)?.ReadChatHistory(ownerId);

            return messages;
        }

        private ChatLimits RequireLimits()
        {
            ChatLimits chatLimits = _chatLimits ?? throw new InvalidOperationException(NotStartedMessage);

            return chatLimits;
        }
    }
}
