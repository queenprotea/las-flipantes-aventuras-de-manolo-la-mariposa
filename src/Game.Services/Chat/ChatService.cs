using System;
using System.Collections.Generic;

using Game.Contracts;

using log4net;

namespace Game.Services
{
    public sealed class ChatService : IChatService
    {
        private const string NotStartedMessage = "The chat capacity was not set; Start must run before the chat is served.";
        private const string MessagePostedMessage = "Member {0} posted a chat message in room {1}.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(ChatService));

        private readonly RoomRegistry _roomRegistry;
        private readonly SessionRegistry _sessionRegistry;
        private readonly TimeProvider _timeProvider;

        private ChatCapacity? _chatCapacity;

        public ChatService(RoomRegistry roomRegistry, SessionRegistry sessionRegistry, TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(roomRegistry);
            ArgumentNullException.ThrowIfNull(sessionRegistry);
            ArgumentNullException.ThrowIfNull(timeProvider);

            _roomRegistry = roomRegistry;
            _sessionRegistry = sessionRegistry;
            _timeProvider = timeProvider;
        }

        public void Start(ChatCapacity chatCapacity)
        {
            ArgumentNullException.ThrowIfNull(chatCapacity);

            _chatCapacity = chatCapacity;
        }

        public SendChatMessageResult SendChatMessage(string text)
        {
            ChatCapacity chatCapacity = RequireCapacity();
            PlayerSession? caller = _sessionRegistry.FindCaller();
            string normalizedText = ChatTextNormalizer.Normalize(text);
            SendChatMessageResult sendResult;
            if (caller is null)
            {
                sendResult = SendChatMessageResult.NotIdentified;
            }
            else if (normalizedText.Length == 0)
            {
                sendResult = SendChatMessageResult.EmptyMessage;
            }
            else if (normalizedText.Length > chatCapacity.MessageMaxLength)
            {
                sendResult = SendChatMessageResult.MessageTooLong;
            }
            else
            {
                sendResult = Post(caller.Owner.OwnerId, normalizedText, chatCapacity);
            }

            return sendResult;
        }

        public ChatHistoryResponse GetChatHistory()
        {
            ChatCapacity chatCapacity = RequireCapacity();
            PlayerSession? caller = _sessionRegistry.FindCaller();
            IReadOnlyList<ChatMessage>? keptMessages = caller is null ? null : ReadHistoryOf(caller.Owner.OwnerId);
            var history = new ChatHistoryResponse
            {
                MessageMaxLength = chatCapacity.MessageMaxLength,
                MessagesKeptPerRoom = chatCapacity.MessagesKeptPerRoom,
            };
            if (keptMessages is not null)
            {
                history.Messages.AddRange(keptMessages);
            }

            return history;
        }

        private SendChatMessageResult Post(int authorOwnerId, string messageText, ChatCapacity chatCapacity)
        {
            var messageDraft = new ChatDraft(messageText, _timeProvider.GetUtcNow().UtcDateTime);
            LiveRoom? authorRoom = _roomRegistry.FindRoomOf(authorOwnerId);
            ChatPosting? posting = authorRoom?.PostChatMessage(authorOwnerId, messageDraft, chatCapacity.MessagesKeptPerRoom);
            if ((authorRoom is null) || (posting is null))
            {
                return SendChatMessageResult.NotInRoom;
            }

            _log.DebugFormat(MessagePostedMessage, posting.Message.SenderMemberId, authorRoom.RoomId);
            var messageNotice = new ChatMessageNotice(posting.Message);
            foreach (int recipientOwnerId in posting.RecipientOwnerIds)
            {
                _sessionRegistry.Deliver(recipientOwnerId, messageNotice);
            }

            return SendChatMessageResult.Sent;
        }

        private IReadOnlyList<ChatMessage>? ReadHistoryOf(int readerOwnerId)
        {
            IReadOnlyList<ChatMessage>? keptMessages = _roomRegistry.FindRoomOf(readerOwnerId)?.ReadChatHistory(readerOwnerId);

            return keptMessages;
        }

        private ChatCapacity RequireCapacity()
        {
            ChatCapacity chatCapacity = _chatCapacity ?? throw new InvalidOperationException(NotStartedMessage);

            return chatCapacity;
        }
    }
}
