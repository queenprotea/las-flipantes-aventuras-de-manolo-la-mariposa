using System.Collections.Generic;

using Game.Contracts;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.UI;

namespace Torres.Client.Widgets
{
    internal sealed class ChatView
    {
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly VerticalStackPanel _messageList = new VerticalStackPanel { Spacing = Metrics.ChatSpacing };
        private readonly ScrollViewer _messageScroller;
        private readonly LimitedTextBox _inputBox = new LimitedTextBox(0);
        private readonly LocalizedLabel _inputHint = new LocalizedLabel(TextKeys.Chat.InputPlaceholder);
        private readonly LocalizedLabel _reportConfirmation = new LocalizedLabel(TextKeys.Chat.ReportSent)
        {
            Font = Fonts.Small,
            TextColor = Theme.MintInk,
            Wrap = true,
            Visible = false,
        };

        private string? _typedMessage;
        private ChatMessage? _reportedMessage;
        private bool _isScrollToEndPending;
        private KeyboardState _previousKeyboardState;

        internal ChatView(int panelWidth)
        {
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
            _typedMessage = null;
            _reportedMessage = null;
            _previousKeyboardState = Keyboard.GetState();
        }

        internal void Update()
        {
            FollowEnterKey();
            _inputHint.Visible = (ReadTypedText().Length == 0) && !HasTypingFocus;
            if (_isScrollToEndPending)
            {
                ScrollToEnd();
            }
        }

        internal string? TakeTypedMessage()
        {
            string? typedMessage = _typedMessage;
            _typedMessage = null;

            return typedMessage;
        }

        internal ChatMessage? TakeReportedMessage()
        {
            ChatMessage? reportedMessage = _reportedMessage;
            _reportedMessage = null;

            return reportedMessage;
        }

        internal void ShowMessages(IReadOnlyList<ChatMessage> messages, HashSet<int> reportableMessageIds)
        {
            _messageList.Widgets.Clear();
            if (messages.Count == 0)
            {
                _messageList.Widgets.Add(new LocalizedLabel(TextKeys.Chat.NoMessages)
                {
                    Font = Fonts.Small,
                    TextColor = Theme.MutedInk,
                    Wrap = true,
                });
            }

            foreach (ChatMessage message in messages)
            {
                _messageList.Widgets.Add(BuildMessageRow(message, reportableMessageIds.Contains(message.MessageId)));
            }

            _isScrollToEndPending = true;
        }

        internal void LimitMessageLength(int messageMaxLength)
        {
            _inputBox.LongestText = messageMaxLength;
        }

        internal void ShowNotice(string titleKey)
        {
            _noticeBox.ShowTitle(titleKey);
        }

        internal void ShowReportConfirmation()
        {
            _reportConfirmation.Visible = true;
        }

        internal void HideNotices()
        {
            _noticeBox.Hide();
            _reportConfirmation.Visible = false;
        }

        internal void ClearSentText(string acceptedText)
        {
            if (ReadTypedText().Trim() == acceptedText)
            {
                _inputBox.Text = string.Empty;
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

            _inputHint.Font = Fonts.Small;
            _inputHint.TextColor = Theme.SoftInk;
            _inputHint.Padding = Metrics.InputPadding;
            _inputHint.VerticalAlignment = VerticalAlignment.Center;
        }

        private VerticalStackPanel BuildRoot(int panelWidth)
        {
            var typingArea = new Panel { HorizontalAlignment = HorizontalAlignment.Stretch };
            typingArea.Widgets.Add(_inputHint);
            typingArea.Widgets.Add(_inputBox);

            var inputArea = new HorizontalStackPanel
            {
                Background = Theme.SurfaceBrush,
                Border = Theme.StrongLineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            inputArea.Widgets.Add(typingArea);
            StackPanel.SetProportionType(typingArea, ProportionType.Fill);
            inputArea.Widgets.Add(BuildSendButton());

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

        private LocalizedSymbolButton BuildSendButton()
        {
            var arrowLabel = new Label
            {
                Text = DisplaySymbols.SendArrow,
                Font = Fonts.Small,
                TextColor = Theme.MintInk,
            };
            var sendButton = new LocalizedSymbolButton(arrowLabel, TextKeys.Chat.SendToolTip)
            {
                Padding = Metrics.InputPadding,
                Background = null,
                OverBackground = null,
                PressedBackground = null,
                Border = null,
                BorderThickness = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
            };
            sendButton.Click += SendButtonOnClick;

            return sendButton;
        }

        private HorizontalStackPanel BuildMessageRow(ChatMessage message, bool isReportable)
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
            if (isReportable)
            {
                messageRow.Widgets.Add(BuildReportButton(message));
            }

            return messageRow;
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
                Tag = message,
            };
            reportButton.Click += ReportButtonOnClick;

            return reportButton;
        }

        private void ReportButtonOnClick(object sender, MyraEventArgs e)
        {
            if (sender is Widget { Tag: ChatMessage reportedMessage })
            {
                _reportedMessage = reportedMessage;
            }
        }

        private void ScrollToEnd()
        {
            _messageScroller.ScrollPosition = new Point(_messageScroller.ScrollPosition.X, _messageScroller.ScrollMaximum.Y);
            _isScrollToEndPending = false;
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
            string typedText = ReadTypedText();
            if (typedText.Trim().Length > 0)
            {
                _typedMessage = typedText;
            }
        }

        private string ReadTypedText()
        {
            string typedText = _inputBox.Text ?? string.Empty;

            return typedText;
        }

        private void SendButtonOnClick(object sender, MyraEventArgs e)
        {
            StartSend();
        }
    }
}
