using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Game.Contracts;

using log4net;

using Npgsql;

namespace Game.Services
{
    public sealed class ChatService : IChatService
    {
        private const int ReportsRecordedAtOnce = 1;
        private const string NotStartedMessage = "The chat capacity was not set; Start must run before the chat is served.";
        private const string MessagePostedMessage = "Member {0} posted a chat message in room {1}.";
        private const string ReportFailedMessage = "The database could not be used while player {0} reported a message in room {1}.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(ChatService));

        private readonly SemaphoreSlim _oneReportAtATimeGate = new SemaphoreSlim(ReportsRecordedAtOnce, ReportsRecordedAtOnce);
        private readonly RoomRegistry _roomRegistry;
        private readonly SessionRegistry _sessionRegistry;
        private readonly ReportRecorder _reportRecorder;
        private readonly TimeProvider _timeProvider;

        private ChatCapacity? _chatCapacity;

        public ChatService(RoomRegistry roomRegistry, SessionRegistry sessionRegistry, ReportRecorder reportRecorder, TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(roomRegistry);
            ArgumentNullException.ThrowIfNull(sessionRegistry);
            ArgumentNullException.ThrowIfNull(reportRecorder);
            ArgumentNullException.ThrowIfNull(timeProvider);

            _roomRegistry = roomRegistry;
            _sessionRegistry = sessionRegistry;
            _reportRecorder = reportRecorder;
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

        /// <inheritdoc/>
        public async Task<ReportChatMessageResult> ReportChatMessageAsync(int roomId, int messageId)
        {
            PlayerSession? caller = _sessionRegistry.FindCaller();
            ReportChatMessageResult reportResult;
            if (caller is null)
            {
                reportResult = ReportChatMessageResult.NotIdentified;
            }
            else if (caller.Owner.IsGuest)
            {
                reportResult = ReportChatMessageResult.ReporterIsGuest;
            }
            else
            {
                reportResult = await ReportOneAtATimeAsync(caller.Owner.OwnerId, roomId, messageId);
            }

            return reportResult;
        }

        private async Task<ReportChatMessageResult> ReportOneAtATimeAsync(int reporterOwnerId, int roomId, int messageId)
        {
            await _oneReportAtATimeGate.WaitAsync();
            ReportChatMessageResult reportResult;
            try
            {
                reportResult = await ReportInRoomAsync(reporterOwnerId, roomId, messageId);
            }
            finally
            {
                _oneReportAtATimeGate.Release();
            }

            return reportResult;
        }

        private async Task<ReportChatMessageResult> ReportInRoomAsync(int reporterOwnerId, int roomId, int messageId)
        {
            LiveRoom? reporterRoom = _roomRegistry.FindRoomOf(reporterOwnerId);
            ReportChatMessageResult reportResult;
            if (reporterRoom is null)
            {
                reportResult = ReportChatMessageResult.NotInRoom;
            }
            else
            {
                ReportTarget? reportTarget = reporterRoom.RoomId == roomId ? reporterRoom.FindReportTarget(reporterOwnerId, messageId) : null;
                reportResult = await ResolveReportTargetAsync(reporterRoom, reporterOwnerId, reportTarget);
            }

            return reportResult;
        }

        private async Task<ReportChatMessageResult> ResolveReportTargetAsync(LiveRoom reporterRoom, int reporterOwnerId, ReportTarget? reportTarget)
        {
            ReportChatMessageResult reportResult;
            if (reportTarget is null)
            {
                reportResult = ReportChatMessageResult.MessageNotFound;
            }
            else
            {
                reportResult = FindResultWithoutRecording(reporterOwnerId, reportTarget) ?? await RecordReportAsync(reporterRoom, reporterOwnerId, reportTarget);
            }

            return reportResult;
        }

        private async Task<ReportChatMessageResult> RecordReportAsync(LiveRoom reporterRoom, int reporterOwnerId, ReportTarget reportTarget)
        {
            ReportChatMessageResult reportResult;
            try
            {
                bool wasRecorded = await _reportRecorder.RecordReportAsync(reporterOwnerId, reportTarget);
                if (wasRecorded)
                {
                    reporterRoom.RecordReporter(reportTarget.MessageId, reporterOwnerId);
                    reportResult = ReportChatMessageResult.Reported;
                }
                else
                {
                    reportResult = ReportChatMessageResult.MessageNotFound;
                }
            }
            catch (NpgsqlException exception)
            {
                _log.Error(string.Format(CultureInfo.InvariantCulture, ReportFailedMessage, reporterOwnerId, reportTarget.RoomId), exception);
                reportResult = ReportChatMessageResult.DatabaseUnavailable;
            }

            return reportResult;
        }

        private static ReportChatMessageResult? FindResultWithoutRecording(int reporterOwnerId, ReportTarget reportTarget)
        {
            ReportChatMessageResult? resultWithoutRecording = null;
            if (reportTarget.IsAuthorGuest)
            {
                resultWithoutRecording = ReportChatMessageResult.AuthorIsGuest;
            }
            else if (reportTarget.AuthorOwnerId == reporterOwnerId)
            {
                resultWithoutRecording = ReportChatMessageResult.OwnMessage;
            }
            else if (reportTarget.WasReportedByReporter)
            {
                resultWithoutRecording = ReportChatMessageResult.Reported;
            }

            return resultWithoutRecording;
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
