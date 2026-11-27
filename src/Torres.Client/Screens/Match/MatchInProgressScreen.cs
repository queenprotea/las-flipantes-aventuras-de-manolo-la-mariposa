using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class MatchInProgressScreen : Screen
    {
        private readonly MatchInProgressFlow _matchInProgressFlow;
        private readonly MatchPause _matchPause;
        private readonly PauseCardFactory _pauseCards;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly LocalizedLabel _offlineMessage = LabelFactory.CreateParagraph(TextKeys.MatchInProgress.LeftWhileOfflineMessage);

        internal MatchInProgressScreen(MatchInProgressFlow matchInProgressFlow, MatchPause matchPause, PauseCardFactory pauseCards)
            : base(TextKeys.MatchInProgress.HeaderLabel, true)
        {
            _matchInProgressFlow = matchInProgressFlow;
            _matchPause = matchPause;
            _pauseCards = pauseCards;
        }

        internal override void Open()
        {
            _noticeBox.Hide();
            _matchInProgressFlow.Open();
            _offlineMessage.TextArguments = new string[] { _matchInProgressFlow.MatchRoomCode };
            MatchView? noMatchShown = null;
            _matchPause.ShowTable(Root, noMatchShown);
        }

        internal override void Update(GameTime gameTime)
        {
            _matchPause.Advance(gameTime.ElapsedGameTime);
            ScreenOutcome? outcome = _matchInProgressFlow.TakeOutcome();
            if (outcome?.Notice is Notice notice)
            {
                DisplayNotice(_noticeBox, notice);
            }

            if (outcome?.NextScreen is ScreenId nextScreen)
            {
                RequestedScreen = nextScreen;
            }
        }

        internal override void RenderScene()
        {
            _matchPause.RenderScene();
        }

        protected override Widget Build()
        {
            LocalizedLabel abandonHint = LabelFactory.CreateCenteredHint(TextKeys.MatchInProgress.AbandonHint, MatchLayout.MatchInProgressCardWidth);
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
            Panel cardLayer = _pauseCards.CreateCardLayer(cardContent, MatchLayout.MatchInProgressCardWidth);

            return cardLayer;
        }

        private HorizontalStackPanel BuildActions()
        {
            LocalizedButton returnButton = ButtonFactory.CreatePrimaryButton(TextKeys.MatchInProgress.ReturnToMatchButton);
            LocalizedButton abandonButton = ButtonFactory.CreateDestructiveButton(TextKeys.MatchInProgress.AbandonButton);
            _pauseCards.StylePrimaryPill(returnButton);
            _pauseCards.StyleDestructivePill(abandonButton);
            returnButton.Click += ReturnButtonOnClick;
            abandonButton.Click += AbandonButtonOnClick;
            HorizontalStackPanel dialogActions = PanelFactory.CreateButtonRow();
            dialogActions.HorizontalAlignment = HorizontalAlignment.Center;
            dialogActions.Widgets.Add(returnButton);
            dialogActions.Widgets.Add(abandonButton);

            return dialogActions;
        }

        private void ReturnButtonOnClick(object sender, MyraEventArgs e)
        {
            if (!_matchInProgressFlow.IsAbandoning)
            {
                RequestedScreen = ScreenId.Resume;
            }
        }

        private void AbandonButtonOnClick(object sender, MyraEventArgs e)
        {
            _noticeBox.Hide();
            _matchInProgressFlow.Abandon();
        }
    }
}
