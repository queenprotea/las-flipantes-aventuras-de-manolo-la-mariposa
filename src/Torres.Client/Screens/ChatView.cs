using System;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class ChatView
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly VerticalStackPanel _messageList = new VerticalStackPanel { Spacing = Metrics.ChatSpacing };
        private readonly ScrollViewer _messageScroller;
        private readonly TextBox _inputBox = new TextBox();
        private readonly LocalizedLabel _inputHint = new LocalizedLabel(TextKeys.Chat.InputPlaceholder);
        private readonly LocalizedLabel _reportConfirmation = new LocalizedLabel(TextKeys.Chat.ReportSent)
        {
            Font = Fonts.Small,
            TextColor = Theme.MintInk,
            Wrap = true,
            Visible = false,
        };

        private Task<ChatHistoryResponse>? _historyRequest;
        private Task<SendChatMessageResult>? _sendRequest;
        private Task<ReportChatMessageResult>? _reportRequest;
        private ChatMessage? _messageBeingReported;
        private int _historyRoomId;
        private string _sentText = string.Empty;
        private int? _shownRevision;
        private bool _isScrollToEndPending;
        private KeyboardState _previousKeyboardState;

        internal ChatView(GameSessionConnection gameSessionConnection, PlayerSession playerSession, int panelWidth)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(playerSession);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(panelWidth);

            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            _messageList.Width = panelWidth - Metrics.ChatPadding.Left - Metrics.ChatPadding.Right - Metrics.ChatScrollbarAllowance;
            _messageScroller = new ScrollViewer
            {
                Content = _messageList,
                ShowVerticalScrollBar = true,
                ShowHorizontalScrollBar = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            ScrollbarLook.Apply(_messageScroller);
            StyleInput();
            Root = BuildRoot(panelWidth);
        }

        internal Widget Root { get; }

        internal bool HasTypingFocus => _inputBox.IsKeyboardFocused;

        internal void Open()
        {
            HideNotices();
            _shownRevision = null;
            _previousKeyboardState = Keyboard.GetState();
            RequestMissingHistory();
        }

        internal void Update()
        {
            FollowHistoryRequest();
            FollowSendRequest();
            FollowReportRequest();
            FollowEnterKey();
            _inputHint.Visible = (ReadTypedText().Length == 0) && !HasTypingFocus;
            if (_playerSession.RoomChat.Revision != _shownRevision)
            {
                RenderMessages();
            }
            else if (_isScrollToEndPending)
            {
                ScrollToEnd();
            }
        }

        private void StyleInput()
        {
            _inputBox.Font = Fonts.Small;
            _inputBox.TextColor = Theme.Ink;
            _inputBox.Padding = Metrics.InputPadding;
            _inputBox.Background = null;
            _inputBox.FocusedBackground = null;
            _inputBox.Border = null;
            _inputBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            _inputBox.ValueChanging += InputBoxOnValueChanging;

            _inputHint.Font = Fonts.Small;
            _inputHint.TextColor = Theme.SoftInk;
            _inputHint.Padding = Metrics.InputPadding;
            _inputHint.VerticalAlignment = VerticalAlignment.Center;
        }

        private VerticalStackPanel BuildRoot(int panelWidth)
        {
            var inputArea = new Panel
            {
                Background = Theme.SurfaceBrush,
                Border = Theme.StrongLineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            inputArea.Widgets.Add(_inputHint);
            inputArea.Widgets.Add(_inputBox);

            var chatPanel = new VerticalStackPanel
            {
                Spacing = Metrics.ChatSpacing,
                Padding = Metrics.ChatPadding,
                Width = panelWidth,
                MinHeight = Metrics.ChatMinHeight,
                Background = Theme.SurfaceSunkenBrush,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            chatPanel.Widgets.Add(_noticeBox);
            chatPanel.Widgets.Add(_reportConfirmation);
            chatPanel.Widgets.Add(_messageScroller);
            StackPanel.SetProportionType(_messageScroller, ProportionType.Fill);
            chatPanel.Widgets.Add(inputArea);

            return chatPanel;
        }

        private void RenderMessages()
        {
            RoomChatLog roomChat = _playerSession.RoomChat;
            _shownRevision = roomChat.Revision;
            _messageList.Widgets.Clear();
            if (roomChat.Messages.Count == 0)
            {
                _messageList.Widgets.Add(new LocalizedLabel(TextKeys.Chat.NoMessages)
                {
                    Font = Fonts.Small,
                    TextColor = Theme.MutedInk,
                    Wrap = true,
                });
            }

            foreach (ChatMessage message in roomChat.Messages)
            {
                _messageList.Widgets.Add(BuildMessageRow(message));
            }

            _isScrollToEndPending = true;
        }

        private HorizontalStackPanel BuildMessageRow(ChatMessage message)
        {
            var authorLabel = new Label
            {
                Text = DisplayFormats.FormatChatAuthor(message.SenderDisplayName),
                Font = Fonts.Small,
                TextColor = Theme.Ink,
                SupportsCommands = false,
            };
            var messageTextLabel = new Label
            {
                Text = message.Text,
                Font = Fonts.Small,
                TextColor = Theme.SoftInk,
                Wrap = true,
                SupportsCommands = false,
            };

            var messageRow = new HorizontalStackPanel
            {
                Spacing = Metrics.ChatAuthorSpacing,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            messageRow.Widgets.Add(authorLabel);
            messageRow.Widgets.Add(messageTextLabel);
            StackPanel.SetProportionType(messageTextLabel, ProportionType.Fill);
            if (IsReportable(message))
            {
                messageRow.Widgets.Add(BuildReportButton(message));
            }

            return messageRow;
        }

        private bool IsReportable(ChatMessage message)
        {
            int? viewerMemberId = _playerSession.CurrentRoom?.ViewerMemberId;
            bool isFromAnotherAccount = !message.SenderIsGuest && (message.SenderMemberId != viewerMemberId);
            bool isReportable = _playerSession.HasAccount && isFromAnotherAccount && !_playerSession.RoomChat.IsReportClosed(message.MessageId);

            return isReportable;
        }

        private LocalizedSymbolButton BuildReportButton(ChatMessage message)
        {
            var flagLabel = new Label
            {
                Text = DisplaySymbols.ReportFlag,
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
            };
            var reportButton = new LocalizedSymbolButton(flagLabel, TextKeys.Chat.ReportToolTip)
            {
                Padding = Metrics.ChatReportButtonPadding,
                Background = null,
                OverBackground = null,
                PressedBackground = null,
                Border = null,
                BorderThickness = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Top,
            };
            reportButton.Click += (sender, e) => StartReport(message);

            return reportButton;
        }

        private void StartReport(ChatMessage message)
        {
            bool canReport = (_reportRequest is null) && _gameSessionConnection.IsSessionOpen;
            if (!canReport)
            {
                return;
            }

            HideNotices();
            _messageBeingReported = message;
            _reportRequest = _gameSessionConnection.ReportChatMessageAsync(message);
        }

        private void FollowReportRequest()
        {
            if ((_reportRequest is null) || !_reportRequest.IsCompleted || (_messageBeingReported is null))
            {
                return;
            }

            Task<ReportChatMessageResult> completedReport = _reportRequest;
            ChatMessage reportedMessage = _messageBeingReported;
            _reportRequest = null;
            _messageBeingReported = null;
            if (completedReport.IsCompletedSuccessfully)
            {
                ShowReportResult(reportedMessage, completedReport.Result);
            }
            else
            {
                _noticeBox.ShowTitle(TextKeys.Common.ServerErrorTitle);
            }
        }

        private void ShowReportResult(ChatMessage reportedMessage, ReportChatMessageResult reportResult)
        {
            if (!ClosesReport(reportResult))
            {
                _noticeBox.ShowTitle(TextKeys.Common.ServerErrorTitle);
                return;
            }

            _playerSession.RoomChat.CloseReport(reportedMessage.RoomId, reportedMessage.MessageId);
            if (reportResult == ReportChatMessageResult.Reported)
            {
                _reportConfirmation.Visible = true;
            }
            else if (reportResult == ReportChatMessageResult.MessageNotFound)
            {
                _noticeBox.ShowTitle(TextKeys.Chat.ReportUnavailable);
            }
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

        private void HideNotices()
        {
            _noticeBox.Hide();
            _reportConfirmation.Visible = false;
        }

        private void ScrollToEnd()
        {
            _messageScroller.ScrollPosition = new Point(_messageScroller.ScrollPosition.X, _messageScroller.ScrollMaximum.Y);
            _isScrollToEndPending = false;
        }

        private void RequestMissingHistory()
        {
            RoomChatLog roomChat = _playerSession.RoomChat;
            int? followedRoomId = roomChat.RoomId;
            bool canRequestHistory = (_historyRequest is null) && !roomChat.IsLoaded && _gameSessionConnection.IsSessionOpen;
            if (canRequestHistory && (followedRoomId is not null))
            {
                _historyRoomId = followedRoomId.Value;
                _historyRequest = _gameSessionConnection.GetChatHistoryAsync();
            }
        }

        private void FollowHistoryRequest()
        {
            if ((_historyRequest is null) || !_historyRequest.IsCompleted)
            {
                return;
            }

            Task<ChatHistoryResponse> completedHistory = _historyRequest;
            _historyRequest = null;
            if (completedHistory.IsCompletedSuccessfully)
            {
                _playerSession.RoomChat.Load(_historyRoomId, completedHistory.Result);
            }
            else
            {
                _noticeBox.ShowTitle(TextKeys.Common.ServerErrorTitle);
            }
        }

        private void FollowEnterKey()
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            bool isEnterPressed = currentKeyboardState.IsKeyDown(Keys.Enter) && _previousKeyboardState.IsKeyUp(Keys.Enter);
            _previousKeyboardState = currentKeyboardState;
            if (isEnterPressed && HasTypingFocus)
            {
                StartSend();
            }
        }

        private void StartSend()
        {
            string typedText = ReadTypedText().Trim();
            bool canSend = (_sendRequest is null) && (typedText.Length > 0) && _gameSessionConnection.IsSessionOpen;
            if (!canSend)
            {
                return;
            }

            HideNotices();
            _sentText = typedText;
            _sendRequest = _gameSessionConnection.SendChatMessageAsync(typedText);
        }

        private void FollowSendRequest()
        {
            if ((_sendRequest is null) || !_sendRequest.IsCompleted)
            {
                return;
            }

            Task<SendChatMessageResult> completedSend = _sendRequest;
            _sendRequest = null;
            if (!completedSend.IsCompletedSuccessfully)
            {
                _noticeBox.ShowTitle(TextKeys.Common.ServerErrorTitle);
            }
            else if (IsAcceptedSend(completedSend.Result))
            {
                ClearSentText();
            }
        }

        private static bool IsAcceptedSend(SendChatMessageResult sendResult)
        {
            bool isAccepted = (sendResult == SendChatMessageResult.Sent) || (sendResult == SendChatMessageResult.EmptyMessage);

            return isAccepted;
        }

        private void ClearSentText()
        {
            if (ReadTypedText().Trim() == _sentText)
            {
                _inputBox.Text = string.Empty;
            }
        }

        private string ReadTypedText()
        {
            string typedText = _inputBox.Text ?? string.Empty;

            return typedText;
        }

        private void InputBoxOnValueChanging(object sender, ValueChangingEventArgs<string> e)
        {
            int messageMaxLength = _playerSession.RoomChat.MessageMaxLength;
            if ((messageMaxLength > 0) && (e.NewValue is not null) && (e.NewValue.Length > messageMaxLength))
            {
                e.Cancel = true;
            }
        }
    }
}
