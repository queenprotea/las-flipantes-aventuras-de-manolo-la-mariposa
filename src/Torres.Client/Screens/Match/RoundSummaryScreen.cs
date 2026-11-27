using System.Linq;

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
    internal sealed class RoundSummaryScreen : Screen
    {
        private readonly MatchSummaryFlow _matchSummaryFlow;
        private readonly MatchPause _matchPause;
        private readonly PauseCardFactory _pauseCards;
        private readonly LocalizedLabel _titleLabel = LabelFactory.CreateCenteredTitle(TextKeys.RoundSummary.RoundFinishedTitle, Fonts.Headline);
        private readonly VerticalStackPanel _scoreRows = new VerticalStackPanel { Spacing = MatchLayout.PauseRowSpacing };
        private readonly HorizontalStackPanel _placerSection = new HorizontalStackPanel();
        private readonly VerticalStackPanel _placerPanel = new VerticalStackPanel
        {
            Spacing = MatchLayout.PauseRowSpacing,
            VerticalAlignment = VerticalAlignment.Center,
        };
        private readonly CountdownView _countdownView;
        private LocalizedLabel? _gardenStaysLabel;
        private LocalizedLabel? _rulesLabel;

        private MatchView? _shownMatchView;

        internal RoundSummaryScreen(MatchSummaryFlow matchSummaryFlow, MatchPause matchPause, PauseCardFactory pauseCards)
            : base(TextKeys.RoundSummary.HeaderLabel, true)
        {
            _matchSummaryFlow = matchSummaryFlow;
            _matchPause = matchPause;
            _pauseCards = pauseCards;
            _countdownView = new CountdownView(matchPause.VolumeTextures, Fonts.Clock, Theme.Ink);
        }

        internal override void Open()
        {
            _shownMatchView = null;
            _countdownView.Restart();
            MatchView? matchView = _matchSummaryFlow.CurrentMatchView;
            _matchPause.ShowTable(Root, matchView);
            if (matchView is null)
            {
                RequestedScreen = ScreenId.MainMenu;
            }
            else
            {
                HeaderArguments = new string[] { DisplayFormats.FormatInteger(matchView.RoundNumber - 1) };
                ShowMatchView(matchView);
            }
        }

        internal override void Update(GameTime gameTime)
        {
            _matchPause.Advance(gameTime.ElapsedGameTime);
            MatchView? matchView = _matchSummaryFlow.CurrentMatchView;
            if (matchView is null)
            {
                return;
            }

            if (!ReferenceEquals(matchView, _shownMatchView))
            {
                ShowMatchView(matchView);
            }

            _countdownView.Show(_matchSummaryFlow.StepTimeLeft);
            if (matchView.Phase == MatchPhase.PlayerTurn)
            {
                RequestedScreen = ScreenId.Match;
            }
            else if (matchView.Phase == MatchPhase.Finished)
            {
                RequestedScreen = ScreenId.Result;
            }
        }

        internal override void RenderScene()
        {
            _matchPause.RenderScene();
        }

        protected override Widget Build()
        {
            _gardenStaysLabel = LabelFactory.CreateCenteredHint(TextKeys.RoundSummary.GardenStaysInstruction, MatchLayout.RoundSummaryCardWidth);
            _rulesLabel = LabelFactory.CreateCenteredHint(TextKeys.RoundSummary.NextRoundRulesInstruction, MatchLayout.RoundSummaryCardWidth);

            var cardContent = new VerticalStackPanel { Spacing = MatchLayout.PauseSectionSpacing };
            cardContent.Widgets.Add(_titleLabel);
            cardContent.Widgets.Add(_gardenStaysLabel);
            cardContent.Widgets.Add(BuildTable());
            cardContent.Widgets.Add(BuildButterflyPlacer());
            cardContent.Widgets.Add(_rulesLabel);
            cardContent.Widgets.Add(BuildContinueRow());
            Panel cardLayer = _pauseCards.CreateCardLayer(cardContent, MatchLayout.RoundSummaryCardWidth);

            return cardLayer;
        }

        private HorizontalStackPanel BuildPlayerRow(MatchSeat seat, MatchRoundScore roundScore, bool isViewer)
        {
            string butterflyText = DisplayFormats.FormatButterflyPoints(roundScore.ButterflyPoints);

            HorizontalStackPanel playerTag = _pauseCards.CreatePlayerTag(seat, Fonts.Control);
            Panel totalCell = _pauseCards.CreateTotalCell(roundScore.PlanterPoints + roundScore.ButterflyPoints);

            HorizontalStackPanel playerRow = _pauseCards.CreateTableRow(isViewer, false);
            playerRow.Widgets.Add(playerTag);
            playerRow.Widgets.Add(PauseCardFactory.CreateStatCell(DisplayFormats.FormatInteger(roundScore.PlanterPoints), Theme.SoftInk));
            playerRow.Widgets.Add(PauseCardFactory.CreateStatCell(butterflyText, Theme.MintInk));
            playerRow.Widgets.Add(totalCell);
            StackPanel.SetProportionType(playerTag, ProportionType.Fill);

            return playerRow;
        }

        private VerticalStackPanel BuildTable()
        {
            HorizontalStackPanel headerRow = _pauseCards.CreateTableRow(false, false);
            headerRow.Background = null;
            LocalizedLabel playerHeader = PauseCardFactory.CreateHeaderCell(TextKeys.RoundSummary.PlayerColumn);
            playerHeader.Width = null;
            playerHeader.TextAlign = TextHorizontalAlignment.Left;
            headerRow.Widgets.Add(playerHeader);
            headerRow.Widgets.Add(PauseCardFactory.CreateHeaderCell(TextKeys.RoundSummary.PlantersColumn));
            headerRow.Widgets.Add(PauseCardFactory.CreateHeaderCell(TextKeys.RoundSummary.ButterflyColumn));
            headerRow.Widgets.Add(PauseCardFactory.CreateHeaderCell(TextKeys.RoundSummary.RoundColumn));
            StackPanel.SetProportionType(playerHeader, ProportionType.Fill);

            var scoreTable = new VerticalStackPanel();
            scoreTable.Widgets.Add(headerRow);
            scoreTable.Widgets.Add(_scoreRows);

            return scoreTable;
        }

        private VerticalStackPanel BuildButterflyPlacer()
        {
            _placerSection.Widgets.Add(_placerPanel);
            _placerSection.Widgets.Add(_countdownView.Root);
            StackPanel.SetProportionType(_placerPanel, ProportionType.Fill);

            var butterflyPlacer = new VerticalStackPanel { Spacing = MatchLayout.PauseSectionSpacing };
            butterflyPlacer.Widgets.Add(PanelFactory.CreateDivider());
            butterflyPlacer.Widgets.Add(_placerSection);

            return butterflyPlacer;
        }

        private HorizontalStackPanel BuildContinueRow()
        {
            LocalizedButton continueButton = ButtonFactory.CreatePrimaryButton(TextKeys.RoundSummary.ContinueButton);
            _pauseCards.StylePrimaryPill(continueButton);
            continueButton.Click += ContinueButtonOnClick;

            HorizontalStackPanel continueRow = PanelFactory.CreateButtonRow();
            continueRow.HorizontalAlignment = HorizontalAlignment.Center;
            continueRow.Widgets.Add(continueButton);

            return continueRow;
        }

        private void ShowMatchView(MatchView matchView)
        {
            _shownMatchView = matchView;
            int closedRoundNumber = matchView.RoundNumber - 1;
            _titleLabel.TextArguments = new string[] { DisplayFormats.FormatInteger(closedRoundNumber) };
            if (_gardenStaysLabel is not null)
            {
                _gardenStaysLabel.TextArguments = new string[] { DisplayFormats.FormatInteger(matchView.RoundNumber) };
            }

            if (_rulesLabel is not null)
            {
                _rulesLabel.TextArguments = new string[] { DisplayFormats.FormatInteger(matchView.RoundNumber), DisplayFormats.FormatInteger(matchView.RoundTurnCount), DisplayFormats.FormatInteger(matchView.RoundButterflyPoints) };
            }

            _scoreRows.Widgets.Clear();
            foreach (MatchSeat seat in matchView.Seats.Where(seat => seat.RoundScores.Count >= closedRoundNumber))
            {
                _scoreRows.Widgets.Add(BuildPlayerRow(seat, seat.RoundScores[closedRoundNumber - 1], seat.SeatNumber == matchView.ViewerSeatNumber));
            }

            ShowButterflyPlacer(matchView);
        }

        private void ShowButterflyPlacer(MatchView matchView)
        {
            bool isPlacingButterfly = matchView.Phase == MatchPhase.ButterflyPlacement;
            _placerSection.Visible = isPlacingButterfly;
            _placerPanel.Widgets.Clear();
            if (isPlacingButterfly)
            {
                MatchSeat placerSeat = matchView.Seats.First(seat => seat.SeatNumber == matchView.ActiveSeatNumber);
                _placerPanel.Widgets.Add(new LocalizedLabel(TextKeys.RoundSummary.PlaceButterflyInstruction)
                {
                    Font = Fonts.Label,
                    TextColor = Theme.MutedInk,
                });
                _placerPanel.Widgets.Add(_pauseCards.CreatePlayerPill(placerSeat));
            }
        }

        private void ContinueButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.InitialPlacement;
        }
    }
}
