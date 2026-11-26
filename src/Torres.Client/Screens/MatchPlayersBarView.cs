using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Scene;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MatchPlayersBarView
    {
        private readonly FigureIcons _figureIcons;
        private readonly VolumeTextures _volumeTextures;
        private readonly HorizontalStackPanel _playersRow = new HorizontalStackPanel { Spacing = MatchLayout.TopBarSpacing };

        internal MatchPlayersBarView(FigureIcons figureIcons, VolumeTextures volumeTextures)
        {
            _figureIcons = figureIcons;
            _volumeTextures = volumeTextures;
        }

        internal Widget Root => _playersRow;

        internal void Show(MatchView matchView)
        {
            _playersRow.Widgets.Clear();
            foreach (MatchSeat seat in matchView.Seats)
            {
                _playersRow.Widgets.Add(BuildPlayerPill(seat, seat.SeatNumber == matchView.ViewerSeatNumber, seat.SeatNumber == matchView.ActiveSeatNumber));
            }
        }

        private static VerticalStackPanel BuildPlayerIdentity(MatchSeat seat, bool isViewer, bool isPlaying)
        {
            var detailRow = new HorizontalStackPanel { Spacing = MatchLayout.TableLabelSpacing };
            if (isViewer)
            {
                detailRow.Widgets.Add(new LocalizedLabel(TextKeys.Common.YouTag) { Font = Fonts.Label, TextColor = Theme.MutedInk });
            }

            if (seat.IsRetired)
            {
                detailRow.Widgets.Add(new LocalizedLabel(TextKeys.Common.WithdrewTag) { Font = Fonts.Label, TextColor = Theme.BlushInk });
            }
            else if (seat.IsDisconnected)
            {
                detailRow.Widgets.Add(new LocalizedLabel(TextKeys.Common.DisconnectedTag) { Font = Fonts.Label, TextColor = Theme.ButterInk });
            }

            if (seat.HandCount > 0)
            {
                if (detailRow.Widgets.Count > 0)
                {
                    detailRow.Widgets.Add(new Label { Text = DisplaySymbols.InlineSeparator, Font = Fonts.Label, TextColor = Theme.MutedInk });
                }

                detailRow.Widgets.Add(new LocalizedLabel(TextKeys.Match.CardsInHandTag) { Font = Fonts.Label, TextColor = Theme.MutedInk, TextArguments = new string[] { DisplayFormats.FormatInteger(seat.HandCount) } });
            }

            Color nameColor = isPlaying ? Theme.MintInk : Theme.Ink;
            var playerIdentity = new VerticalStackPanel { VerticalAlignment = VerticalAlignment.Center };
            playerIdentity.Widgets.Add(new Label { Text = seat.DisplayName, Font = Fonts.Small, TextColor = nameColor });
            playerIdentity.Widgets.Add(detailRow);

            return playerIdentity;
        }

        private HorizontalStackPanel BuildPlayerPill(MatchSeat seat, bool isViewer, bool isPlaying)
        {
            VerticalStackPanel playerIdentity = BuildPlayerIdentity(seat, isViewer, isPlaying);
            var playerPill = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerPillPadding,
                Width = MatchLayout.PlayerPillWidth,
                Background = isPlaying ? _volumeTextures.ActivePillBrush : _volumeTextures.PillBrush,
            };
            playerPill.Widgets.Add(MatchPieces.CreateCaterpillarImage(_figureIcons, seat.Color));
            playerPill.Widgets.Add(playerIdentity);
            playerPill.Widgets.Add(MatchPieces.CreateScoreCircle(_volumeTextures, seat.FinalScore));
            StackPanel.SetProportionType(playerIdentity, ProportionType.Fill);

            return playerPill;
        }
    }
}
