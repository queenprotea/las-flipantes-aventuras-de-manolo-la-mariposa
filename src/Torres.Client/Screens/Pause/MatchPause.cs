using System;

using FontStashSharp;

using Game.Contracts;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Scene;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MatchPause : IDisposable
    {
        private readonly MatchBoardLayoutFactory _layoutFactory = new MatchBoardLayoutFactory();
        private readonly TableBackdrop _tableBackdrop;

        internal MatchPause(GraphicsDevice graphicsDevice)
        {
            VolumeTextures = new VolumeTextures(graphicsDevice);
            FigureIcons = new FigureIcons(graphicsDevice);
            _tableBackdrop = new TableBackdrop(graphicsDevice);
        }

        internal VolumeTextures VolumeTextures { get; }

        internal FigureIcons FigureIcons { get; }

        internal Panel CreateCardLayer(Widget cardContent, int contentWidth)
        {
            var pauseCard = new Panel
            {
                Width = contentWidth + MatchLayout.PauseCardPadding.Left + MatchLayout.PauseCardPadding.Right,
                Padding = MatchLayout.PauseCardPadding,
                Background = VolumeTextures.CardBrush,
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

        internal void ShowTable(Widget cardLayer, MatchView? matchView)
        {
            _tableBackdrop.RemoveFromParent();
            if (cardLayer is Panel layerPanel)
            {
                layerPanel.Widgets.Insert(0, _tableBackdrop);
            }

            if (matchView is null)
            {
                _tableBackdrop.ShowGarden(BoardLayout.Empty, null);
                return;
            }

            BoardCell? butterflyCell = null;
            if (matchView.HasButterfly)
            {
                butterflyCell = MatchCellConversion.ToBoardCell(matchView.ButterflyCell);
            }

            _tableBackdrop.ShowGarden(_layoutFactory.Create(matchView), butterflyCell);
        }

        internal void Advance(TimeSpan elapsedTime)
        {
            _tableBackdrop.Advance(elapsedTime);
        }

        internal void RenderScene()
        {
            FigureIcons.RenderOnce();
            _tableBackdrop.RenderScene();
        }

        internal HorizontalStackPanel CreatePlayerTag(MatchSeat seat, SpriteFontBase nameFont)
        {
            var playerTag = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                VerticalAlignment = VerticalAlignment.Center,
            };
            playerTag.Widgets.Add(MatchPieces.CreateCaterpillarImage(FigureIcons, seat.Color));
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
                Background = VolumeTextures.PillBrush,
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
                Background = VolumeTextures.PillBrush,
            };

            return statusPill;
        }

        internal HorizontalStackPanel CreateTableRow(bool isViewer, bool isWinner)
        {
            IBrush? rowBrush = null;
            if (isWinner)
            {
                rowBrush = VolumeTextures.WinnerPillBrush;
            }
            else if (isViewer)
            {
                rowBrush = VolumeTextures.ActivePillBrush;
            }

            var tableRow = new HorizontalStackPanel
            {
                Padding = MatchLayout.PauseRowPadding,
                Background = rowBrush,
            };

            return tableRow;
        }

        internal Panel CreateScoreCircle(int score)
        {
            Panel scoreCircle = MatchPieces.CreateScoreCircle(VolumeTextures, score);

            return scoreCircle;
        }

        internal void StylePrimaryPill(LocalizedButton pillButton)
        {
            StylePill(pillButton, VolumeTextures.ChosenPillBrush, VolumeTextures.ChosenPillBrush, Theme.Surface);
            pillButton.DisabledBackground = VolumeTextures.HoveredPillBrush;
        }

        internal void StyleSecondaryPill(LocalizedButton pillButton)
        {
            StylePill(pillButton, VolumeTextures.PillBrush, VolumeTextures.HoveredPillBrush, Theme.Ink);
        }

        internal void StyleDestructivePill(LocalizedButton pillButton)
        {
            StylePill(pillButton, VolumeTextures.PillBrush, VolumeTextures.PillBrush, Theme.BlushInk);
        }

        public void Dispose()
        {
            _tableBackdrop.Dispose();
            FigureIcons.Dispose();
            VolumeTextures.Dispose();
        }

        private static void StylePill(LocalizedButton pillButton, IBrush restingBrush, IBrush hoveredBrush, Color labelColor)
        {
            pillButton.Background = restingBrush;
            pillButton.OverBackground = hoveredBrush;
            pillButton.PressedBackground = hoveredBrush;
            pillButton.Border = null;
            pillButton.Padding = MatchLayout.PausePillPadding;
            pillButton.LabelColor = labelColor;
        }
    }
}
