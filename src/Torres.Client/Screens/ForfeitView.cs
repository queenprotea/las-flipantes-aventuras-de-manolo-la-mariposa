using System;
using System.Threading.Tasks;

using Game.Contracts;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class ForfeitView
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly NoticeBox _questionNotice = new NoticeBox();
        private readonly HorizontalStackPanel _answerRow = Screen.CreateButtonRow();

        private Task<ForfeitResult>? _forfeitRequest;

        internal ForfeitView(GameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(playerSession);

            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            LocalizedButton forfeitButton = CreateSmallButton(Screen.CreateDestructiveButton(TextKeys.Match.ForfeitButton));
            forfeitButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            forfeitButton.Click += ForfeitButtonOnClick;
            LocalizedButton confirmButton = CreateSmallButton(Screen.CreateDestructiveButton(TextKeys.Alerts.ForfeitConfirmButton));
            confirmButton.Click += ConfirmButtonOnClick;
            LocalizedButton cancelButton = CreateSmallButton(Screen.CreateSecondaryButton(TextKeys.Common.CancelButton));
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
            _forfeitRequest = null;
            HideQuestion();
        }

        internal ScreenId? TakeNextScreen()
        {
            ScreenId? nextScreen = null;
            if ((_forfeitRequest is not null) && _forfeitRequest.IsCompleted)
            {
                Task<ForfeitResult> completedForfeit = _forfeitRequest;
                _forfeitRequest = null;
                nextScreen = FollowForfeit(completedForfeit);
            }

            return nextScreen;
        }

        private static LocalizedButton CreateSmallButton(LocalizedButton styledButton)
        {
            styledButton.LabelFont = Fonts.Small;
            styledButton.Padding = Metrics.SmallButtonPadding;

            return styledButton;
        }

        private ScreenId? FollowForfeit(Task<ForfeitResult> completedForfeit)
        {
            ScreenId? nextScreen = null;
            bool hasLeftMatch = completedForfeit.IsCompletedSuccessfully && (completedForfeit.Result != ForfeitResult.NotIdentified);
            if (hasLeftMatch)
            {
                HideQuestion();
                _playerSession.ForfeitMatch();
                nextScreen = _playerSession.CurrentRoom is null ? ScreenId.MainMenu : ScreenId.Room;
            }
            else
            {
                string[]? noDetailArguments = null;
                _answerRow.Visible = false;
                _questionNotice.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, noDetailArguments);
            }

            return nextScreen;
        }

        private void HideQuestion()
        {
            _questionNotice.Hide();
            _answerRow.Visible = false;
        }

        private void ForfeitButtonOnClick(object sender, MyraEventArgs e)
        {
            if (_forfeitRequest is null)
            {
                string[]? noBodyArguments = null;
                _questionNotice.ShowMessage(TextKeys.Alerts.ForfeitTitle, TextKeys.Alerts.ForfeitBody, noBodyArguments);
                _answerRow.Visible = true;
            }
        }

        private void ConfirmButtonOnClick(object sender, MyraEventArgs e)
        {
            _answerRow.Visible = false;
            _forfeitRequest ??= _gameSessionConnection.ForfeitAsync();
        }

        private void CancelButtonOnClick(object sender, MyraEventArgs e)
        {
            HideQuestion();
        }
    }
}
