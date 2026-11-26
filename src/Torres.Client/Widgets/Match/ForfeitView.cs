using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.UI;

namespace Torres.Client.Widgets
{
    internal sealed class ForfeitView
    {
        private readonly NoticeBox _questionNotice = new NoticeBox();
        private readonly HorizontalStackPanel _answerRow = PanelFactory.CreateButtonRow();

        private bool _isConfirmed;
        private bool _isWaiting;

        internal ForfeitView(string forfeitButtonKey)
        {
            LocalizedButton forfeitButton = CreateSmallButton(ButtonFactory.CreateDestructiveButton(forfeitButtonKey));
            forfeitButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            forfeitButton.Click += ForfeitButtonOnClick;
            LocalizedButton confirmButton = CreateSmallButton(ButtonFactory.CreateDestructiveButton(TextKeys.Alerts.ForfeitConfirmButton));
            confirmButton.Click += ConfirmButtonOnClick;
            LocalizedButton cancelButton = CreateSmallButton(ButtonFactory.CreateSecondaryButton(TextKeys.Common.CancelButton));
            cancelButton.Click += CancelButtonOnClick;
            _answerRow.Widgets.Add(confirmButton);
            _answerRow.Widgets.Add(cancelButton);

            var forfeitPanel = new VerticalStackPanel
            {
                Spacing = Metrics.ButtonSpacing,
            };
            forfeitPanel.Widgets.Add(forfeitButton);
            forfeitPanel.Widgets.Add(_questionNotice);
            forfeitPanel.Widgets.Add(_answerRow);
            Root = forfeitPanel;
            HideQuestion();
        }

        internal Widget Root { get; }

        internal void Reset()
        {
            _isConfirmed = false;
            _isWaiting = false;
            HideQuestion();
        }

        internal bool TakeConfirmation()
        {
            bool isConfirmed = _isConfirmed;
            _isConfirmed = false;

            return isConfirmed;
        }

        internal void ShowWaiting(bool isWaiting)
        {
            _isWaiting = isWaiting;
        }

        internal void ShowServerError()
        {
            string[]? noDetailArguments = null;
            _answerRow.Visible = false;
            _questionNotice.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, noDetailArguments);
        }

        internal void HideQuestion()
        {
            _questionNotice.Hide();
            _answerRow.Visible = false;
        }

        private static LocalizedButton CreateSmallButton(LocalizedButton styledButton)
        {
            styledButton.LabelFont = Fonts.Small;
            styledButton.Padding = Metrics.SmallButtonPadding;

            return styledButton;
        }

        private void ForfeitButtonOnClick(object sender, MyraEventArgs e)
        {
            if (!_isWaiting)
            {
                string[]? noBodyArguments = null;
                _questionNotice.ShowMessage(TextKeys.Alerts.ForfeitTitle, TextKeys.Alerts.ForfeitBody, noBodyArguments);
                _answerRow.Visible = true;
            }
        }

        private void ConfirmButtonOnClick(object sender, MyraEventArgs e)
        {
            _answerRow.Visible = false;
            _isConfirmed = true;
        }

        private void CancelButtonOnClick(object sender, MyraEventArgs e)
        {
            HideQuestion();
        }
    }
}
