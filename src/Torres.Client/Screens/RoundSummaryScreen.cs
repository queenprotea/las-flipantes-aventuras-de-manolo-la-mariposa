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
        private const string NoPointsMark = "—";
        private const string BonusSign = "+";

        private static readonly Thickness _headerRowPadding = new Thickness(0, 0, 0, TableCellSpacing);
        private static readonly Thickness _playerRowPadding = new Thickness(0, TableCellSpacing);
        private static readonly Thickness _butterflyPadding = new Thickness(0, ButterflyVerticalPadding);

        private static readonly RoundPlayer[] _previewPlayers =
        {
            new RoundPlayer("jesus", true, 24, 0, Theme.MintBrush),
            new RoundPlayer("valentin", false, 30, 10, Theme.LavenderBrush),
            new RoundPlayer("salma", false, 18, 0, Theme.ButterBrush),
            new RoundPlayer("scarleth", false, 12, 10, Theme.SkyBrush),
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

            LocalizedLabel gardenStays = Hint(TextKeys.RoundSummary.GardenStaysInstruction);
            gardenStays.Font = Fonts.Body;
            gardenStays.TextAlign = TextHorizontalAlignment.Center;
            gardenStays.HorizontalAlignment = HorizontalAlignment.Center;
            gardenStays.Margin = new Thickness(0, 0, 0, TitleBottomSpacing);
            gardenStays.TextArguments = new object[] { NextRound };

            LocalizedLabel rules = Hint(TextKeys.RoundSummary.Round3RulesInstruction);
            rules.TextAlign = TextHorizontalAlignment.Center;
            rules.HorizontalAlignment = HorizontalAlignment.Center;
            rules.Margin = new Thickness(0, RulesTopSpacing, 0, 0);

            var content = new VerticalStackPanel
            {
                Width = Sizes.RoundSummaryWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            content.Widgets.Add(title);
            content.Widgets.Add(gardenStays);
            content.Widgets.Add(BuildTable());
            content.Widgets.Add(BuildButterflyPlacer());
            content.Widgets.Add(rules);

            return content;
        }

        private static VerticalStackPanel BuildTable()
        {
            var header = new HorizontalStackPanel
            {
                Padding = _headerRowPadding,
            };
            LocalizedLabel playerHeader = BuildHeaderCell(TextKeys.RoundSummary.PlayerColumn);
            playerHeader.Width = null;
            header.Widgets.Add(playerHeader);
            header.Widgets.Add(BuildHeaderCell(TextKeys.RoundSummary.PlantersColumn));
            header.Widgets.Add(BuildHeaderCell(TextKeys.RoundSummary.ButterflyColumn));
            header.Widgets.Add(BuildHeaderCell(TextKeys.RoundSummary.RoundColumn));
            StackPanel.SetProportionType(playerHeader, ProportionType.Fill);

            var table = new VerticalStackPanel();
            table.Widgets.Add(header);
            foreach (RoundPlayer player in _previewPlayers)
            {
                table.Widgets.Add(Divider());
                table.Widgets.Add(BuildPlayerRow(player));
            }

            return table;
        }

        private static LocalizedLabel BuildHeaderCell(string textKey)
        {
            return new LocalizedLabel(textKey)
            {
                Width = Sizes.RankingStatColumnWidth,
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
            };
        }

        private static HorizontalStackPanel BuildPlayerRow(RoundPlayer player)
        {
            HorizontalStackPanel playerCell = BuildPlayerCell(player, Fonts.Control);
            string butterflyText = player.ButterflyPoints > 0
                ? BonusSign + player.ButterflyPoints.ToString(CultureInfo.CurrentCulture)
                : NoPointsMark;

            var row = new HorizontalStackPanel
            {
                Padding = _playerRowPadding,
            };
            row.Widgets.Add(playerCell);
            row.Widgets.Add(BuildStatCell(player.PlanterPoints.ToString(CultureInfo.CurrentCulture), Theme.SoftInk));
            row.Widgets.Add(BuildStatCell(butterflyText, Theme.SoftInk));
            row.Widgets.Add(BuildStatCell(player.RoundPoints.ToString(CultureInfo.CurrentCulture), Theme.Ink));
            StackPanel.SetProportionType(playerCell, ProportionType.Fill);
            if (player.IsYou)
            {
                row.Background = Theme.MintTintBrush;
            }

            return row;
        }

        private static HorizontalStackPanel BuildPlayerCell(RoundPlayer player, SpriteFontBase font)
        {
            Label name = PlayerNameLabel(player.Name, Theme.Ink);
            name.Font = font;

            var cell = new HorizontalStackPanel
            {
                Spacing = PlayerCellSpacing,
            };
            cell.Widgets.Add(new Panel
            {
                Width = PlayerColorSize,
                Height = PlayerColorSize,
                Background = player.ColorBrush,
                VerticalAlignment = VerticalAlignment.Center,
            });
            cell.Widgets.Add(name);

            return cell;
        }

        private static Label BuildStatCell(string text, Color textColor)
        {
            return new Label
            {
                Text = text,
                Width = Sizes.RankingStatColumnWidth,
                Font = Fonts.Body,
                TextColor = textColor,
                VerticalAlignment = VerticalAlignment.Center,
            };
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

            var row = new HorizontalStackPanel
            {
                Spacing = ButterflySpacing,
                Padding = _butterflyPadding,
            };
            row.Widgets.Add(new Panel
            {
                Width = ButterflyIconSize,
                Height = ButterflyIconSize,
                Background = Theme.LavenderTintBrush,
                Border = Theme.LavenderLineBrush,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            });
            row.Widgets.Add(placer);

            var section = new VerticalStackPanel
            {
                Margin = new Thickness(0, ButterflyTopSpacing, 0, 0),
            };
            section.Widgets.Add(Divider());
            section.Widgets.Add(row);
            section.Widgets.Add(Divider());

            return section;
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
