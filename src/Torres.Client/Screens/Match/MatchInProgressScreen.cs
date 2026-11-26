using FontStashSharp.RichText;

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
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly LocalizedLabel _offlineMessage = LabelFactory.CreateParagraph(TextKeys.MatchInProgress.LeftWhileOfflineMessage);

        internal MatchInProgressScreen(MatchInProgressFlow matchInProgressFlow, MatchPause matchPause)
            : base(TextKeys.MatchInProgress.HeaderLabel, true)
        {
            _matchInProgressFlow = matchInProgressFlow;
            _matchPause = matchPause;
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
            LocalizedLabel abandonHint = LabelFactory.CreateHint(TextKeys.MatchInProgress.AbandonHint);
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
            LocalizedButton returnButton = ButtonFactory.CreatePrimaryButton(TextKeys.MatchInProgress.ReturnToMatchButton);
            LocalizedButton abandonButton = ButtonFactory.CreateDestructiveButton(TextKeys.MatchInProgress.AbandonButton);
            _matchPause.StylePrimaryPill(returnButton);
            _matchPause.StyleDestructivePill(abandonButton);
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
