using System.Globalization;

using FontStashSharp;
using FontStashSharp.RichText;

using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class RoundSummaryScreen : Screen
    {
        private const int PreviewRound = 2;
        private const int NextRound = PreviewRound + 1;
        private const double PreviewSecondsBeforeMatch = 5;

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

        private static readonly Thickness _headerRowPadding = new Thickness(0, 0, 0, TableCellSpacing);
        private static readonly Thickness _playerRowPadding = new Thickness(0, TableCellSpacing);
        private static readonly Thickness _butterflyPadding = new Thickness(0, ButterflyVerticalPadding);

        private static readonly RoundPlayer[] _previewPlayers =
        {
            new RoundPlayer(PreviewSamples.SeatOnePlayer, true, 24, 0, Theme.MintBrush),
            new RoundPlayer(PreviewSamples.SeatTwoPlayer, false, 30, 10, Theme.LavenderBrush),
            new RoundPlayer(PreviewSamples.SeatThreePlayer, false, 18, 0, Theme.ButterBrush),
            new RoundPlayer(PreviewSamples.SeatFourPlayer, false, 12, 10, Theme.SkyBrush),
        };

        private double _secondsShown;

        internal RoundSummaryScreen()
            : base(TextKeys.RoundSummary.HeaderLabel, true)
        {
            HeaderArguments = new object[] { PreviewRound };
        }

        internal override void Open()
        {
            _secondsShown = 0;
        }

        internal override void Update(GameTime gameTime)
        {
            _secondsShown += gameTime.ElapsedGameTime.TotalSeconds;
            if (_secondsShown < PreviewSecondsBeforeMatch)
            {
                return;
            }

            RequestedScreen = ScreenId.Match;
        }

        protected override Widget Build()
        {
            var title = new LocalizedLabel(TextKeys.RoundSummary.RoundFinishedTitle)
            {
                Font = Fonts.Headline,
                TextColor = Theme.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextArguments = new object[] { PreviewRound },
            };

            LocalizedLabel gardenStays = CreateHint(TextKeys.RoundSummary.GardenStaysInstruction);
            gardenStays.Font = Fonts.Body;
            gardenStays.TextAlign = TextHorizontalAlignment.Center;
            gardenStays.HorizontalAlignment = HorizontalAlignment.Center;
            gardenStays.Margin = new Thickness(0, 0, 0, TitleBottomSpacing);
            gardenStays.TextArguments = new object[] { NextRound };

            LocalizedLabel rules = CreateHint(TextKeys.RoundSummary.Round3RulesInstruction);
            rules.TextAlign = TextHorizontalAlignment.Center;
            rules.HorizontalAlignment = HorizontalAlignment.Center;
            rules.Margin = new Thickness(0, RulesTopSpacing, 0, 0);

            var screenLayout = new VerticalStackPanel
            {
                Width = Sizes.RoundSummaryWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            screenLayout.Widgets.Add(title);
            screenLayout.Widgets.Add(gardenStays);
            screenLayout.Widgets.Add(BuildTable());
            screenLayout.Widgets.Add(BuildButterflyPlacer());
            screenLayout.Widgets.Add(rules);

            return screenLayout;
        }

        private static VerticalStackPanel BuildTable()
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
            foreach (RoundPlayer player in _previewPlayers)
            {
                scoreTable.Widgets.Add(CreateDivider());
                scoreTable.Widgets.Add(BuildPlayerRow(player));
            }

            return scoreTable;
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

        private static HorizontalStackPanel BuildPlayerRow(RoundPlayer player)
        {
            HorizontalStackPanel playerCell = BuildPlayerCell(player, Fonts.Control);
            string butterflyText = player.ButterflyPoints > 0
                ? DisplayFormats.FormatBonus(player.ButterflyPoints)
                : DisplaySymbols.MissingValue;

            var playerRow = new HorizontalStackPanel
            {
                Padding = _playerRowPadding,
            };
            playerRow.Widgets.Add(playerCell);
            playerRow.Widgets.Add(BuildStatCell(player.PlanterPoints.ToString(CultureInfo.CurrentCulture), Theme.SoftInk));
            playerRow.Widgets.Add(BuildStatCell(butterflyText, Theme.SoftInk));
            playerRow.Widgets.Add(BuildStatCell(player.RoundPoints.ToString(CultureInfo.CurrentCulture), Theme.Ink));
            StackPanel.SetProportionType(playerCell, ProportionType.Fill);
            if (player.IsYou)
            {
                playerRow.Background = Theme.MintTintBrush;
            }

            return playerRow;
        }

        private static HorizontalStackPanel BuildPlayerCell(RoundPlayer player, SpriteFontBase font)
        {
            Label nameLabel = CreatePlayerNameLabel(player.Name, Theme.Ink);
            nameLabel.Font = font;

            var playerCell = new HorizontalStackPanel
            {
                Spacing = PlayerCellSpacing,
            };
            playerCell.Widgets.Add(new Panel
            {
                Width = PlayerColorSize,
                Height = PlayerColorSize,
                Background = player.ColorBrush,
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

        private static VerticalStackPanel BuildButterflyPlacer()
        {
            HorizontalStackPanel placerCell = BuildPlayerCell(FindButterflyPlacer(), Fonts.Body);
            placerCell.Margin = new Thickness(0, ButterflyNameTopSpacing, 0, 0);

            var placer = new VerticalStackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
            };
            placer.Widgets.Add(new LocalizedLabel(TextKeys.RoundSummary.PlaceButterflyInstruction)
            {
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
            });
            placer.Widgets.Add(placerCell);

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
            placerRow.Widgets.Add(placer);

            var butterflyPlacer = new VerticalStackPanel
            {
                Margin = new Thickness(0, ButterflyTopSpacing, 0, 0),
            };
            butterflyPlacer.Widgets.Add(CreateDivider());
            butterflyPlacer.Widgets.Add(placerRow);
            butterflyPlacer.Widgets.Add(CreateDivider());

            return butterflyPlacer;
        }

        private static RoundPlayer FindButterflyPlacer()
        {
            RoundPlayer placer = _previewPlayers[0];
            foreach (RoundPlayer player in _previewPlayers)
            {
                if (player.RoundPoints < placer.RoundPoints)
                {
                    placer = player;
                }
            }

            return placer;
        }

        private sealed record RoundPlayer(string Name, bool IsYou, int PlanterPoints, int ButterflyPoints, IBrush ColorBrush)
        {
            internal int RoundPoints => PlanterPoints + ButterflyPoints;
        }
    }
}
