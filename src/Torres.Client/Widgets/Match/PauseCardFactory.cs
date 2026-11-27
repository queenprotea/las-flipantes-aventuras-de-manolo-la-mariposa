using FontStashSharp;
using FontStashSharp.RichText;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Scene;
using Torres.Client.UI;

namespace Torres.Client.Widgets
{
    internal sealed class PauseCardFactory
    {
        private readonly VolumeTextures _volumeTextures;
        private readonly FigureIcons _figureIcons;
        private readonly PillStyle _primaryPillStyle;
        private readonly PillStyle _secondaryPillStyle;
        private readonly PillStyle _destructivePillStyle;

        internal PauseCardFactory(VolumeTextures volumeTextures, FigureIcons figureIcons)
        {
            _volumeTextures = volumeTextures;
            _figureIcons = figureIcons;
            _primaryPillStyle = new PillStyle(volumeTextures.ChosenPillBrush, volumeTextures.ChosenPillBrush, Theme.Surface);
            _secondaryPillStyle = new PillStyle(volumeTextures.PillBrush, volumeTextures.HoveredPillBrush, Theme.Ink);
            _destructivePillStyle = new PillStyle(volumeTextures.PillBrush, volumeTextures.PillBrush, Theme.BlushInk);
        }

        internal static LocalizedLabel CreateHeaderCell(string textKey)
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

        internal static Label CreateStatCell(string cellText, Color textColor)
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

        internal Panel CreateCardLayer(Widget cardContent, int contentWidth)
        {
            var pauseCard = new Panel
            {
                Width = contentWidth + MatchLayout.PauseCardPadding.Left + MatchLayout.PauseCardPadding.Right,
                Padding = MatchLayout.PauseCardPadding,
                Background = _volumeTextures.CardBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            pauseCard.Widgets.Add(cardContent);
            var cardLayer = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            cardLayer.Widgets.Add(pauseCard);

            return cardLayer;
        }

        internal HorizontalStackPanel CreatePlayerTag(MatchSeat seat, SpriteFontBase nameFont)
        {
            var playerTag = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                VerticalAlignment = VerticalAlignment.Center,
            };
            playerTag.Widgets.Add(PlayerBadges.CreateCaterpillarImage(_figureIcons, seat.Color));
            playerTag.Widgets.Add(new Label
            {
                Text = seat.DisplayName,
                Font = nameFont,
                TextColor = Theme.Ink,
                VerticalAlignment = VerticalAlignment.Center,
                SupportsCommands = false,
            });

            return playerTag;
        }

        internal HorizontalStackPanel CreatePlayerPill(MatchSeat seat)
        {
            var playerPill = new HorizontalStackPanel
            {
                Width = MatchLayout.PlayerPillWidth,
                Padding = MatchLayout.PlayerPillPadding,
                Background = _volumeTextures.PillBrush,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            playerPill.Widgets.Add(CreatePlayerTag(seat, Fonts.Body));

            return playerPill;
        }

        internal HorizontalStackPanel CreateStatusPill()
        {
            var statusPill = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerPillPadding,
                Background = _volumeTextures.PillBrush,
            };

            return statusPill;
        }

        internal HorizontalStackPanel CreateTableRow(bool isViewer, bool isWinner)
        {
            IBrush? rowBrush = null;
            if (isWinner)
            {
                rowBrush = _volumeTextures.WinnerPillBrush;
            }
            else if (isViewer)
            {
                rowBrush = _volumeTextures.ActivePillBrush;
            }

            var tableRow = new HorizontalStackPanel
            {
                Padding = MatchLayout.PauseRowPadding,
                Background = rowBrush,
            };

            return tableRow;
        }

        internal Panel CreateTotalCell(int score)
        {
            Panel totalCircle = PlayerBadges.CreateScoreCircle(_volumeTextures, score);
            totalCircle.HorizontalAlignment = HorizontalAlignment.Center;
            var totalCell = new Panel { Width = MatchLayout.PauseStatColumnWidth };
            totalCell.Widgets.Add(totalCircle);

            return totalCell;
        }

        internal void StylePrimaryPill(LocalizedButton pillButton)
        {
            StylePill(pillButton, _primaryPillStyle);
            pillButton.DisabledBackground = _volumeTextures.HoveredPillBrush;
        }

        internal void StyleSecondaryPill(LocalizedButton pillButton)
        {
            StylePill(pillButton, _secondaryPillStyle);
        }

        internal void StyleDestructivePill(LocalizedButton pillButton)
        {
            StylePill(pillButton, _destructivePillStyle);
        }

        private static void StylePill(LocalizedButton pillButton, PillStyle pillStyle)
        {
            pillButton.Background = pillStyle.RestingBrush;
            pillButton.OverBackground = pillStyle.HoveredBrush;
            pillButton.PressedBackground = pillStyle.HoveredBrush;
            pillButton.Border = null;
            pillButton.Padding = MatchLayout.PausePillPadding;
            pillButton.LabelColor = pillStyle.LabelColor;
        }
    }
}
