using System.Threading.Tasks;

using FontStashSharp.RichText;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MatchInProgressScreen : Screen
    {
        private const int TitleBottomSpacing = 6;
        private const int MessageBottomSpacing = 14;
        private const int HintTopSpacing = 12;

        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly LocalizedLabel _offlineMessage = CreateParagraph(TextKeys.MatchInProgress.LeftWhileOfflineMessage);

        private Task<ForfeitResult>? _forfeitRequest;

        internal MatchInProgressScreen(GameSessionConnection gameSessionConnection, PlayerSession playerSession)
            : base(TextKeys.MatchInProgress.HeaderLabel, true)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal override void Open()
        {
            _noticeBox.Hide();
            _forfeitRequest = null;
            _offlineMessage.TextArguments = new[] { _playerSession.MatchRoomCode ?? string.Empty };
        }

        internal override void Update(GameTime gameTime)
        {
            if ((_forfeitRequest is null) || !_forfeitRequest.IsCompleted)
            {
                return;
            }

            Task<ForfeitResult> completedForfeit = _forfeitRequest;
            _forfeitRequest = null;
            bool hasLeftMatch = completedForfeit.IsCompletedSuccessfully && (completedForfeit.Result != ForfeitResult.NotIdentified);
            if (hasLeftMatch)
            {
                _playerSession.ForfeitMatch();
                RequestedScreen = ScreenId.MainMenu;
            }
            else
            {
                string[]? noDetailArguments = null;
                _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, noDetailArguments);
            }
        }

        protected override Widget Build()
        {
            LocalizedLabel abandonHint = CreateHint(TextKeys.MatchInProgress.AbandonHint);
            abandonHint.TextAlign = TextHorizontalAlignment.Center;
            abandonHint.HorizontalAlignment = HorizontalAlignment.Center;
            abandonHint.Margin = new Thickness(0, HintTopSpacing, 0, 0);
            _noticeBox.HorizontalAlignment = HorizontalAlignment.Center;

            var screenLayout = new VerticalStackPanel
            {
                Width = Sizes.MatchInProgressWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            screenLayout.Widgets.Add(BuildDialog());
            screenLayout.Widgets.Add(abandonHint);
            screenLayout.Widgets.Add(_noticeBox);

            return screenLayout;
        }

        private VerticalStackPanel BuildDialog()
        {
            LocalizedButton returnButton = CreatePrimaryButton(TextKeys.MatchInProgress.ReturnToMatchButton);
            LocalizedButton abandonButton = CreateDestructiveButton(TextKeys.MatchInProgress.AbandonButton);
            returnButton.Click += ReturnButtonOnClick;
            abandonButton.Click += AbandonButtonOnClick;

            HorizontalStackPanel dialogActions = CreateButtonRow();
            dialogActions.Widgets.Add(returnButton);
            dialogActions.Widgets.Add(abandonButton);

            _offlineMessage.Margin = new Thickness(0, 0, 0, MessageBottomSpacing);

            VerticalStackPanel dialog = CreateCard(Sizes.DialogWidth);
            dialog.Spacing = 0;
            dialog.HorizontalAlignment = HorizontalAlignment.Center;
            dialog.Widgets.Add(new LocalizedLabel(TextKeys.MatchInProgress.Title)
            {
                Font = Fonts.Title,
                TextColor = Theme.Ink,
                Margin = new Thickness(0, 0, 0, TitleBottomSpacing),
            });
            dialog.Widgets.Add(_offlineMessage);
            dialog.Widgets.Add(dialogActions);

            return dialog;
        }

        private void ReturnButtonOnClick(object sender, MyraEventArgs e)
        {
            if (_forfeitRequest is null)
            {
                RequestedScreen = ScreenId.Resume;
            }
        }

        private void AbandonButtonOnClick(object sender, MyraEventArgs e)
        {
            _noticeBox.Hide();
            _forfeitRequest ??= _gameSessionConnection.ForfeitAsync();
        }
    }
}
