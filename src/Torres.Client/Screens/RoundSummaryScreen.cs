using System;
using System.Linq;

using FontStashSharp;
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
    internal sealed class RoundSummaryScreen : Screen
    {
        private const int TitleBottomSpacing = 20;
        private const int TableCellSpacing = 9;
        private const int PlayerCellSpacing = 8;
        private const int PlayerColorSize = 20;
        private const int ButterflyTopSpacing = 24;
        private const int ButterflySpacing = 12;
        private const int ButterflyVerticalPadding = 14;
        private const int ButterflyIconSize = 34;
        private const int ButterflyNameTopSpacing = 3;
        private const int RulesTopSpacing = 14;
        private const int ContinueTopSpacing = 18;

        private static readonly Thickness _headerRowPadding = new Thickness(0, 0, 0, TableCellSpacing);
        private static readonly Thickness _playerRowPadding = new Thickness(0, TableCellSpacing);
        private static readonly Thickness _butterflyPadding = new Thickness(0, ButterflyVerticalPadding);

        private readonly PlayerSession _playerSession;
        private readonly LocalizedLabel _titleLabel = new LocalizedLabel(TextKeys.RoundSummary.RoundFinishedTitle) { Font = Fonts.Headline, TextColor = Theme.Ink, HorizontalAlignment = HorizontalAlignment.Center };
        private readonly VerticalStackPanel _scoreRows = new VerticalStackPanel();
        private readonly VerticalStackPanel _placerPanel = new VerticalStackPanel { VerticalAlignment = VerticalAlignment.Center };
        private readonly Label _clockLabel = new Label { Font = Fonts.Body, TextColor = Theme.MintInk, VerticalAlignment = VerticalAlignment.Center };
        private LocalizedLabel? _gardenStaysLabel;
        private LocalizedLabel? _rulesLabel;

        private MatchView? _shownMatchView;

        internal RoundSummaryScreen(PlayerSession playerSession)
            : base(TextKeys.RoundSummary.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(playerSession);

            _playerSession = playerSession;
        }

        internal override void Open()
        {
            _shownMatchView = null;
            MatchView? matchView = _playerSession.CurrentMatchView;
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
            MatchView? matchView = _playerSession.CurrentMatchView;
            if (matchView is null)
            {
                return;
            }

            if (!ReferenceEquals(matchView, _shownMatchView))
            {
                ShowMatchView(matchView);
            }

            _clockLabel.Text = FormatStepTimeLeft();
            if (matchView.Phase == MatchPhase.PlayerTurn)
            {
                RequestedScreen = ScreenId.Match;
            }
            else if (matchView.Phase == MatchPhase.Finished)
            {
                RequestedScreen = ScreenId.Result;
            }
        }

        protected override Widget Build()
        {
            _gardenStaysLabel = CreateHint(TextKeys.RoundSummary.GardenStaysInstruction);
            _gardenStaysLabel.Font = Fonts.Body;
            _gardenStaysLabel.TextAlign = TextHorizontalAlignment.Center;
            _gardenStaysLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _gardenStaysLabel.Margin = new Thickness(0, 0, 0, TitleBottomSpacing);

            _rulesLabel = CreateHint(TextKeys.RoundSummary.NextRoundRulesInstruction);
            _rulesLabel.TextAlign = TextHorizontalAlignment.Center;
            _rulesLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _rulesLabel.Margin = new Thickness(0, RulesTopSpacing, 0, 0);

            var screenLayout = new VerticalStackPanel
            {
                Width = Sizes.RoundSummaryWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            screenLayout.Widgets.Add(_titleLabel);
            screenLayout.Widgets.Add(_gardenStaysLabel);
            screenLayout.Widgets.Add(BuildTable());
            screenLayout.Widgets.Add(BuildButterflyPlacer());
            screenLayout.Widgets.Add(_rulesLabel);
            screenLayout.Widgets.Add(BuildContinueRow());

            return screenLayout;
        }

        private static LocalizedLabel BuildHeaderCell(string textKey)
        {
            var headerCell = new LocalizedLabel(textKey)
            {
                Width = Sizes.RankingStatColumnWidth,
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
            };

            return headerCell;
        }

        private static HorizontalStackPanel BuildPlayerRow(MatchSeat seat, MatchRoundScore roundScore, bool isViewer)
        {
            HorizontalStackPanel playerCell = BuildPlayerCell(seat, Fonts.Control);
            string butterflyText = DisplaySymbols.MissingValue;
            if (roundScore.ButterflyPoints > 0)
            {
                butterflyText = DisplayFormats.FormatBonus(roundScore.ButterflyPoints);
            }

            var playerRow = new HorizontalStackPanel
            {
                Padding = _playerRowPadding,
            };
            playerRow.Widgets.Add(playerCell);
            playerRow.Widgets.Add(BuildStatCell(DisplayFormats.FormatInteger(roundScore.PlanterPoints), Theme.SoftInk));
            playerRow.Widgets.Add(BuildStatCell(butterflyText, Theme.SoftInk));
            playerRow.Widgets.Add(BuildStatCell(DisplayFormats.FormatInteger(roundScore.PlanterPoints + roundScore.ButterflyPoints), Theme.Ink));
            StackPanel.SetProportionType(playerCell, ProportionType.Fill);
            if (isViewer)
            {
                playerRow.Background = Theme.MintTintBrush;
            }

            return playerRow;
        }

        private static HorizontalStackPanel BuildPlayerCell(MatchSeat seat, SpriteFontBase font)
        {
            Label nameLabel = CreatePlayerNameLabel(seat.DisplayName, Theme.Ink);
            nameLabel.Font = font;

            var playerCell = new HorizontalStackPanel
            {
                Spacing = PlayerCellSpacing,
            };
            playerCell.Widgets.Add(new Panel
            {
                Width = PlayerColorSize,
                Height = PlayerColorSize,
                Background = SeatColors.BrushFor(seat.Color),
                VerticalAlignment = VerticalAlignment.Center,
            });
            playerCell.Widgets.Add(nameLabel);

            return playerCell;
        }

        private static Label BuildStatCell(string cellText, Color textColor)
        {
            var statCell = new Label
            {
                Text = cellText,
                Width = Sizes.RankingStatColumnWidth,
                Font = Fonts.Body,
                TextColor = textColor,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return statCell;
        }

        private VerticalStackPanel BuildTable()
        {
            var headerRow = new HorizontalStackPanel
            {
                Padding = _headerRowPadding,
            };
            LocalizedLabel playerHeader = BuildHeaderCell(TextKeys.RoundSummary.PlayerColumn);
            playerHeader.Width = null;
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
            var placerRow = new HorizontalStackPanel
            {
                Spacing = ButterflySpacing,
                Padding = _butterflyPadding,
            };
            placerRow.Widgets.Add(new Panel
            {
                Width = ButterflyIconSize,
                Height = ButterflyIconSize,
                Background = Theme.LavenderTintBrush,
                Border = Theme.LavenderLineBrush,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            });
            placerRow.Widgets.Add(_placerPanel);
            placerRow.Widgets.Add(_clockLabel);
            StackPanel.SetProportionType(_placerPanel, ProportionType.Fill);

            var butterflyPlacer = new VerticalStackPanel
            {
                Margin = new Thickness(0, ButterflyTopSpacing, 0, 0),
            };
            butterflyPlacer.Widgets.Add(CreateDivider());
            butterflyPlacer.Widgets.Add(placerRow);
            butterflyPlacer.Widgets.Add(CreateDivider());

            return butterflyPlacer;
        }

        private HorizontalStackPanel BuildContinueRow()
        {
            LocalizedButton continueButton = CreatePrimaryButton(TextKeys.RoundSummary.ContinueButton);
            continueButton.Click += ContinueButtonOnClick;

            HorizontalStackPanel continueRow = CreateButtonRow();
            continueRow.Margin = new Thickness(0, ContinueTopSpacing, 0, 0);
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
                _scoreRows.Widgets.Add(CreateDivider());
                _scoreRows.Widgets.Add(BuildPlayerRow(seat, seat.RoundScores[closedRoundNumber - 1], seat.SeatNumber == matchView.ViewerSeatNumber));
            }

            _placerPanel.Widgets.Clear();
            if (matchView.Phase == MatchPhase.ButterflyPlacement)
            {
                MatchSeat placerSeat = matchView.Seats.First(seat => seat.SeatNumber == matchView.ActiveSeatNumber);
                HorizontalStackPanel placerCell = BuildPlayerCell(placerSeat, Fonts.Body);
                placerCell.Margin = new Thickness(0, ButterflyNameTopSpacing, 0, 0);
                _placerPanel.Widgets.Add(new LocalizedLabel(TextKeys.RoundSummary.PlaceButterflyInstruction) { Font = Fonts.Label, TextColor = Theme.MutedInk });
                _placerPanel.Widgets.Add(placerCell);
            }
        }

        private string FormatStepTimeLeft()
        {
            TimeSpan stepTimeLeft = TimeSpan.Zero;
            if (_playerSession.StepEndsAtUtc is DateTime stepEndsAtUtc)
            {
                stepTimeLeft = stepEndsAtUtc - DateTime.UtcNow;
            }

            string clockText = DisplayFormats.FormatCountdown(stepTimeLeft);

            return clockText;
        }

        private void ContinueButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.InitialPlacement;
        }
    }
}
