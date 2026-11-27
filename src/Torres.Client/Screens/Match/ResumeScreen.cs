using System.Linq;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class ResumeScreen : Screen
    {
        private readonly ResumeFlow _resumeFlow;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly MatchPause _matchPause;
        private readonly PauseCardFactory _pauseCards;
        private readonly VerticalStackPanel _playerRows = new VerticalStackPanel { Spacing = MatchLayout.PauseRowSpacing };
        private readonly CountdownView _countdownView;

        private MatchView? _shownMatchView;

        internal ResumeScreen(ResumeFlow resumeFlow, MatchPause matchPause, PauseCardFactory pauseCards)
            : base(TextKeys.Resume.HeaderLabel, true)
        {
            _resumeFlow = resumeFlow;
            _matchPause = matchPause;
            _pauseCards = pauseCards;
            _countdownView = new CountdownView(matchPause.VolumeTextures, Fonts.Clock, Theme.MintInk);
            _countdownView.Root.HorizontalAlignment = HorizontalAlignment.Center;
        }

        internal bool WasMatchLost { get; private set; }

        internal override void Open()
        {
            _noticeBox.Hide();
            _shownMatchView = null;
            WasMatchLost = false;
            _countdownView.Restart();
            _playerRows.Widgets.Clear();
            _matchPause.ShowTable(Root, _resumeFlow.CurrentMatchView);
            _resumeFlow.Open();
        }

        internal override void Update(GameTime gameTime)
        {
            _matchPause.Advance(gameTime.ElapsedGameTime);
            _countdownView.Show(_resumeFlow.FindResumeTimeLeft());
            ResumeOutcome? outcome = _resumeFlow.TakeOutcome();
            if (outcome?.Notice is Notice notice)
            {
                DisplayNotice(_noticeBox, notice);
            }

            WasMatchLost |= outcome?.IsMatchLost ?? false;
            MatchView? matchView = _resumeFlow.CurrentMatchView;
            if ((matchView is not null) && !ReferenceEquals(matchView, _shownMatchView) && (matchView.Phase == MatchPhase.AwaitingPlayers))
            {
                _shownMatchView = matchView;
                ShowPlayers(matchView);
            }
        }

        internal override void RenderScene()
        {
            _matchPause.RenderScene();
        }

        internal void ClearRequests()
        {
            WasMatchLost = false;
        }

        protected override Widget Build()
        {
            LocalizedLabel noActionNeeded = LabelFactory.CreateCenteredHint(TextKeys.Resume.NoActionNeededMessage, MatchLayout.ResumeCardWidth);
            LocalizedLabel discardedTurn = LabelFactory.CreateCenteredHint(TextKeys.Resume.DiscardedTurnMessage, MatchLayout.ResumeCardWidth);
            _noticeBox.HorizontalAlignment = HorizontalAlignment.Center;

            var titleBlock = new VerticalStackPanel();
            titleBlock.Widgets.Add(LabelFactory.CreateCenteredTitle(TextKeys.Resume.Title, Fonts.Notice));
            titleBlock.Widgets.Add(new LocalizedLabel(TextKeys.Resume.WaitingStatus)
            {
                Font = Fonts.Body,
                TextColor = Theme.MutedInk,
                HorizontalAlignment = HorizontalAlignment.Center,
            });

            var hintBlock = new VerticalStackPanel();
            hintBlock.Widgets.Add(noActionNeeded);
            hintBlock.Widgets.Add(discardedTurn);

            var cardContent = new VerticalStackPanel { Spacing = MatchLayout.PauseSectionSpacing };
            cardContent.Widgets.Add(titleBlock);
            cardContent.Widgets.Add(_countdownView.Root);
            cardContent.Widgets.Add(PanelFactory.CreateDivider());
            cardContent.Widgets.Add(_playerRows);
            cardContent.Widgets.Add(hintBlock);
            cardContent.Widgets.Add(_noticeBox);
            Panel cardLayer = _pauseCards.CreateCardLayer(cardContent, MatchLayout.ResumeCardWidth);

            return cardLayer;
        }

        private void ShowPlayers(MatchView matchView)
        {
            _playerRows.Widgets.Clear();
            foreach (MatchSeat seat in matchView.Seats.Where(seat => !seat.IsRetired))
            {
                _playerRows.Widgets.Add(BuildPlayerRow(seat));
            }
        }

        private HorizontalStackPanel BuildPlayerRow(MatchSeat seat)
        {
            bool hasReturned = !seat.IsDisconnected;
            HorizontalStackPanel playerTag = _pauseCards.CreatePlayerTag(seat, Fonts.Body);
            HorizontalStackPanel playerRow = _pauseCards.CreateStatusPill();
            playerRow.Widgets.Add(playerTag);
            playerRow.Widgets.Add(new Label
            {
                Text = hasReturned ? DisplaySymbols.CheckMark : DisplaySymbols.WaitingClock,
                Font = Fonts.Body,
                TextColor = hasReturned ? Theme.MintInk : Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            });
            StackPanel.SetProportionType(playerTag, ProportionType.Fill);

            return playerRow;
        }
    }
}
