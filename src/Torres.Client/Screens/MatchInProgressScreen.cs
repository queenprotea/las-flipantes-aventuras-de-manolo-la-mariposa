using System.Threading.Tasks;

using FontStashSharp.RichText;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MatchInProgressScreen : Screen
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly MatchPause _matchPause;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly LocalizedLabel _offlineMessage = CreateParagraph(TextKeys.MatchInProgress.LeftWhileOfflineMessage);

        private Task<ForfeitResult>? _forfeitRequest;

        internal MatchInProgressScreen(GameSessionConnection gameSessionConnection, PlayerSession playerSession, MatchPause matchPause)
            : base(TextKeys.MatchInProgress.HeaderLabel, true)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            _matchPause = matchPause;
        }

        internal override void Open()
        {
            _noticeBox.Hide();
            _forfeitRequest = null;
            _offlineMessage.TextArguments = new[] { _playerSession.MatchRoomCode ?? string.Empty };
            MatchView? noMatchShown = null;
            _matchPause.ShowTable(Root, noMatchShown);
        }

        internal override void Update(GameTime gameTime)
        {
            _matchPause.Advance(gameTime.ElapsedGameTime);
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

        internal override void RenderScene()
        {
            _matchPause.RenderScene();
        }

        protected override Widget Build()
        {
            LocalizedLabel abandonHint = CreateHint(TextKeys.MatchInProgress.AbandonHint);
            abandonHint.Width = MatchLayout.MatchInProgressCardWidth;
            abandonHint.Wrap = true;
            abandonHint.TextAlign = TextHorizontalAlignment.Center;
            abandonHint.HorizontalAlignment = HorizontalAlignment.Center;
            _noticeBox.HorizontalAlignment = HorizontalAlignment.Center;
            _offlineMessage.Width = MatchLayout.MatchInProgressCardWidth;

            var cardContent = new VerticalStackPanel { Spacing = MatchLayout.PauseSectionSpacing };
            cardContent.Widgets.Add(new LocalizedLabel(TextKeys.MatchInProgress.Title)
            {
                Font = Fonts.Title,
                TextColor = Theme.Ink,
            });
            cardContent.Widgets.Add(_offlineMessage);
            cardContent.Widgets.Add(BuildActions());
            cardContent.Widgets.Add(abandonHint);
            cardContent.Widgets.Add(_noticeBox);
            Panel cardLayer = _matchPause.CreateCardLayer(cardContent, MatchLayout.MatchInProgressCardWidth);

            return cardLayer;
        }

        private HorizontalStackPanel BuildActions()
        {
            LocalizedButton returnButton = CreatePrimaryButton(TextKeys.MatchInProgress.ReturnToMatchButton);
            LocalizedButton abandonButton = CreateDestructiveButton(TextKeys.MatchInProgress.AbandonButton);
            _matchPause.StylePrimaryPill(returnButton);
            _matchPause.StyleDestructivePill(abandonButton);
            returnButton.Click += ReturnButtonOnClick;
            abandonButton.Click += AbandonButtonOnClick;
            HorizontalStackPanel dialogActions = CreateButtonRow();
            dialogActions.HorizontalAlignment = HorizontalAlignment.Center;
            dialogActions.Widgets.Add(returnButton);
            dialogActions.Widgets.Add(abandonButton);

            return dialogActions;
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
