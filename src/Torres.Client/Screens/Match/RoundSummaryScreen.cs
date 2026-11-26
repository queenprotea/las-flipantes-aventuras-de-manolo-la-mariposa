using System;
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
        private readonly LocalizedLabel _titleLabel = new LocalizedLabel(TextKeys.RoundSummary.RoundFinishedTitle) { Font = Fonts.Headline, TextColor = Theme.Ink, HorizontalAlignment = HorizontalAlignment.Center };
        private readonly VerticalStackPanel _scoreRows = new VerticalStackPanel { Spacing = MatchLayout.PauseRowSpacing };
        private readonly HorizontalStackPanel _placerSection = new HorizontalStackPanel();
        private readonly VerticalStackPanel _placerPanel = new VerticalStackPanel { Spacing = MatchLayout.PauseRowSpacing, VerticalAlignment = VerticalAlignment.Center };
        private readonly CountdownView _countdownView;
        private LocalizedLabel? _gardenStaysLabel;
        private LocalizedLabel? _rulesLabel;

        private MatchView? _shownMatchView;

        internal RoundSummaryScreen(MatchSummaryFlow matchSummaryFlow, MatchPause matchPause)
            : base(TextKeys.RoundSummary.HeaderLabel, true)
        {
            _matchSummaryFlow = matchSummaryFlow;
            _matchPause = matchPause;
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

            _countdownView.Show(FindStepTimeLeft());
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
            _gardenStaysLabel = CreateCenteredHint(TextKeys.RoundSummary.GardenStaysInstruction);
            _rulesLabel = CreateCenteredHint(TextKeys.RoundSummary.NextRoundRulesInstruction);

            var cardContent = new VerticalStackPanel { Spacing = MatchLayout.PauseSectionSpacing };
            cardContent.Widgets.Add(_titleLabel);
            cardContent.Widgets.Add(_gardenStaysLabel);
            cardContent.Widgets.Add(BuildTable());
            cardContent.Widgets.Add(BuildButterflyPlacer());
            cardContent.Widgets.Add(_rulesLabel);
            cardContent.Widgets.Add(BuildContinueRow());
            Panel cardLayer = _matchPause.CreateCardLayer(cardContent, MatchLayout.RoundSummaryCardWidth);

            return cardLayer;
        }

        private static LocalizedLabel CreateCenteredHint(string textKey)
        {
            LocalizedLabel centeredHint = LabelFactory.CreateHint(textKey);
            centeredHint.Width = MatchLayout.RoundSummaryCardWidth;
            centeredHint.Wrap = true;
            centeredHint.TextAlign = TextHorizontalAlignment.Center;
            centeredHint.HorizontalAlignment = HorizontalAlignment.Center;

            return centeredHint;
        }

        private static LocalizedLabel BuildHeaderCell(string textKey)
        {
            var headerCell = new LocalizedLabel(textKey)
            {
                Width = MatchLayout.PauseStatColumnWidth,
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
                TextAlign = TextHorizontalAlignment.Center,
            };

            return headerCell;
        }

        private static Label BuildStatCell(string cellText, Color textColor)
        {
            var statCell = new Label
            {
                Text = cellText,
                Width = MatchLayout.PauseStatColumnWidth,
                Font = Fonts.Body,
                TextColor = textColor,
                TextAlign = TextHorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return statCell;
        }

        private HorizontalStackPanel BuildPlayerRow(MatchSeat seat, MatchRoundScore roundScore, bool isViewer)
        {
            string butterflyText = DisplaySymbols.MissingValue;
            if (roundScore.ButterflyPoints > 0)
            {
                butterflyText = DisplayFormats.FormatBonus(roundScore.ButterflyPoints);
            }

            HorizontalStackPanel playerTag = _matchPause.CreatePlayerTag(seat, Fonts.Control);
            var totalCell = new Panel { Width = MatchLayout.PauseStatColumnWidth };
            Panel totalCircle = _matchPause.CreateScoreCircle(roundScore.PlanterPoints + roundScore.ButterflyPoints);
            totalCircle.HorizontalAlignment = HorizontalAlignment.Center;
            totalCell.Widgets.Add(totalCircle);

            HorizontalStackPanel playerRow = _matchPause.CreateTableRow(isViewer, false);
            playerRow.Widgets.Add(playerTag);
            playerRow.Widgets.Add(BuildStatCell(DisplayFormats.FormatInteger(roundScore.PlanterPoints), Theme.SoftInk));
            playerRow.Widgets.Add(BuildStatCell(butterflyText, Theme.MintInk));
            playerRow.Widgets.Add(totalCell);
            StackPanel.SetProportionType(playerTag, ProportionType.Fill);

            return playerRow;
        }

        private VerticalStackPanel BuildTable()
        {
            HorizontalStackPanel headerRow = _matchPause.CreateTableRow(false, false);
            headerRow.Background = null;
            LocalizedLabel playerHeader = BuildHeaderCell(TextKeys.RoundSummary.PlayerColumn);
            playerHeader.Width = null;
            playerHeader.TextAlign = TextHorizontalAlignment.Left;
            headerRow.Widgets.Add(playerHeader);
            headerRow.Widgets.Add(BuildHeaderCell(TextKeys.RoundSummary.PlantersColumn));
            headerRow.Widgets.Add(BuildHeaderCell(TextKeys.RoundSummary.ButterflyColumn));
            headerRow.Widgets.Add(BuildHeaderCell(TextKeys.RoundSummary.RoundColumn));
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
            _matchPause.StylePrimaryPill(continueButton);
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
                _placerPanel.Widgets.Add(new LocalizedLabel(TextKeys.RoundSummary.PlaceButterflyInstruction) { Font = Fonts.Label, TextColor = Theme.MutedInk });
                _placerPanel.Widgets.Add(_matchPause.CreatePlayerPill(placerSeat));
            }
        }

        private TimeSpan FindStepTimeLeft()
        {
            TimeSpan stepTimeLeft = TimeSpan.Zero;
            if (_matchSummaryFlow.StepEndsAt is DateTime stepEndsAt)
            {
                stepTimeLeft = stepEndsAt - DateTime.UtcNow;
            }

            return stepTimeLeft;
        }

        private void ContinueButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.InitialPlacement;
        }
    }
}
