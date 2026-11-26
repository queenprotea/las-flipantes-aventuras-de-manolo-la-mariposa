using Game.Contracts;

using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class ChatSection
    {
        private readonly ChatFlow _chatFlow;
        private readonly ChatView _chatView;

        private int? _shownRevision;

        internal ChatSection(ChatFlow chatFlow, int panelWidth)
        {
            _chatFlow = chatFlow;
            _chatView = new ChatView(panelWidth);
        }

        internal Widget Root => _chatView.Root;

        internal bool HasTypingFocus => _chatView.HasTypingFocus;

        internal void Open()
        {
            _shownRevision = null;
            _chatView.Open();
            _chatFlow.Open();
        }

        internal void Update()
        {
            _chatView.Update();
            FollowIntents();
            ChatOutcome? outcome = _chatFlow.TakeOutcome();
            if (outcome is not null)
            {
                ShowOutcome(outcome);
            }

            if (_chatFlow.Revision != _shownRevision)
            {
                _shownRevision = _chatFlow.Revision;
                _chatView.MessageMaxLength = _chatFlow.MessageMaxLength;
                _chatView.ShowMessages(_chatFlow.Messages, _chatFlow.FindReportableMessageIds());
            }
        }

        private void FollowIntents()
        {
            string? typedMessage = _chatView.TakeTypedMessage();
            if ((typedMessage is not null) && _chatFlow.Send(typedMessage))
            {
                _chatView.HideNotices();
            }

            ChatMessage? reportedMessage = _chatView.TakeReportedMessage();
            if ((reportedMessage is not null) && _chatFlow.Report(reportedMessage))
            {
                _chatView.HideNotices();
            }
        }

        private void ShowOutcome(ChatOutcome outcome)
        {
            if (outcome.NoticeKey is string noticeKey)
            {
                _chatView.ShowNotice(noticeKey);
            }

            if (outcome.IsReported)
            {
                _chatView.ShowReportConfirmation();
            }

            if (outcome.AcceptedText is string acceptedText)
            {
                _chatView.ClearSentText(acceptedText);
            }
        }
    }
}
