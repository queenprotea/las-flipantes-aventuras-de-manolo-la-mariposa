using System;
using System.Collections.Generic;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class ChatFlow
    {
        private readonly IGameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<ChatHistoryResponse> _historyRequest = new PendingRequest<ChatHistoryResponse>();
        private readonly PendingRequest<SendChatMessageResult> _sendRequest = new PendingRequest<SendChatMessageResult>();
        private readonly PendingRequest<ReportChatMessageResult> _reportRequest = new PendingRequest<ReportChatMessageResult>();

        private ChatMessage? _messageBeingReported;
        private int _historyRoomId;
        private string _sentText = string.Empty;
        private string? _noticeKey;
        private bool _isReported;
        private string? _acceptedText;

        internal ChatFlow(IGameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal IReadOnlyList<ChatMessage> Messages => _playerSession.RoomChat.Messages;

        internal int Revision => _playerSession.RoomChat.Revision;

        internal int MessageMaxLength => _playerSession.RoomChat.MessageMaxLength;

        internal void Open()
        {
            RoomChatLog roomChat = _playerSession.RoomChat;
            int? followedRoomId = roomChat.RoomId;
            bool canRequestHistory = !_historyRequest.IsWaiting && !roomChat.IsLoaded && _gameSessionConnection.IsSessionOpen;
            if (canRequestHistory && (followedRoomId is int roomId))
            {
                _historyRoomId = roomId;
                _historyRequest.Start(_gameSessionConnection.GetChatHistoryAsync());
            }
        }

        internal HashSet<int> FindReportableMessageIds()
        {
            var reportableMessageIds = new HashSet<int>();
            foreach (ChatMessage message in Messages)
            {
                if (IsReportable(message))
                {
                    reportableMessageIds.Add(message.MessageId);
                }
            }

            return reportableMessageIds;
        }

        internal bool Send(string typedText)
        {
            string trimmedText = typedText.Trim();
            bool canSend = !_sendRequest.IsWaiting && (trimmedText.Length > 0) && _gameSessionConnection.IsSessionOpen;
            if (canSend)
            {
                _sentText = trimmedText;
                _sendRequest.Start(_gameSessionConnection.SendChatMessageAsync(trimmedText));
            }

            return canSend;
        }

        internal bool Report(ChatMessage message)
        {
            bool canReport = !_reportRequest.IsWaiting && _gameSessionConnection.IsSessionOpen;
            if (canReport)
            {
                _messageBeingReported = message;
                _reportRequest.Start(_gameSessionConnection.ReportChatMessageAsync(message));
            }

            return canReport;
        }

        internal ChatOutcome? TakeOutcome()
        {
            FollowHistoryRequest();
            FollowSendRequest();
            FollowReportRequest();
            ChatOutcome? outcome = null;
            if ((_noticeKey is not null) || _isReported || (_acceptedText is not null))
            {
                outcome = new ChatOutcome(_noticeKey, _isReported, _acceptedText);
            }

            _noticeKey = null;
            _isReported = false;
            _acceptedText = null;

            return outcome;
        }

        private static bool ClosesReport(ReportChatMessageResult reportResult)
        {
            bool closesReport = reportResult switch
            {
                ReportChatMessageResult.Reported => true,
                ReportChatMessageResult.MessageNotFound => true,
                ReportChatMessageResult.AuthorIsGuest => true,
                ReportChatMessageResult.OwnMessage => true,
                ReportChatMessageResult.NotIdentified => false,
                ReportChatMessageResult.ReporterIsGuest => false,
                ReportChatMessageResult.NotInRoom => false,
                ReportChatMessageResult.DatabaseUnavailable => false,
                _ => throw new ArgumentOutOfRangeException(nameof(reportResult), reportResult, null),
            };

            return closesReport;
        }

        private static bool IsAcceptedSend(SendChatMessageResult sendResult)
        {
            bool isAccepted = (sendResult == SendChatMessageResult.Sent) || (sendResult == SendChatMessageResult.EmptyMessage);

            return isAccepted;
        }

        private bool IsReportable(ChatMessage message)
        {
            int? viewerMemberId = _playerSession.CurrentRoom?.ViewerMemberId;
            bool isFromAnotherAccount = !message.SenderIsGuest && (message.SenderMemberId != viewerMemberId);
            bool isReportable = _playerSession.HasAccount && isFromAnotherAccount && !_playerSession.RoomChat.IsReportClosed(message.MessageId);

            return isReportable;
        }

        private void FollowHistoryRequest()
        {
            FinishedRequest<ChatHistoryResponse>? finishedHistory = _historyRequest.TakeFinished();
            if ((finishedHistory is not null) && finishedHistory.IsSuccessful && (finishedHistory.Reply is ChatHistoryResponse history))
            {
                _playerSession.RoomChat.Load(_historyRoomId, history);
            }
            else if (finishedHistory is not null)
            {
                _noticeKey = TextKeys.Common.ServerErrorTitle;
            }
        }

        private void FollowSendRequest()
        {
            FinishedRequest<SendChatMessageResult>? finishedSend = _sendRequest.TakeFinished();
            if ((finishedSend is not null) && !finishedSend.IsSuccessful)
            {
                _noticeKey = TextKeys.Common.ServerErrorTitle;
            }
            else if ((finishedSend is not null) && IsAcceptedSend(finishedSend.Reply))
            {
                _acceptedText = _sentText;
            }
        }

        private void FollowReportRequest()
        {
            FinishedRequest<ReportChatMessageResult>? finishedReport = (_messageBeingReported is null) ? null : _reportRequest.TakeFinished();
            if ((finishedReport is not null) && (_messageBeingReported is ChatMessage reportedMessage))
            {
                _messageBeingReported = null;
                FollowReportResult(reportedMessage, finishedReport);
            }
        }

        private void FollowReportResult(ChatMessage reportedMessage, FinishedRequest<ReportChatMessageResult> finishedReport)
        {
            if (!finishedReport.IsSuccessful || !ClosesReport(finishedReport.Reply))
            {
                _noticeKey = TextKeys.Common.ServerErrorTitle;
            }
            else
            {
                _playerSession.RoomChat.CloseReport(reportedMessage.RoomId, reportedMessage.MessageId);
                _isReported = finishedReport.Reply == ReportChatMessageResult.Reported;
                if (finishedReport.Reply == ReportChatMessageResult.MessageNotFound)
                {
                    _noticeKey = TextKeys.Chat.ReportUnavailable;
                }
            }
        }
    }
}
