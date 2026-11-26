using Game.Contracts;

using Myra.Graphics2D.UI;

using Torres.Client.Scene;

namespace Torres.Client.UI
{
    internal static class MatchPieces
    {
        internal static Image CreateCaterpillarImage(FigureIcons figureIcons, SeatColor seatColor)
        {
            var caterpillarImage = new Image
            {
                Width = MatchLayout.PlayerIconWidth,
                Height = MatchLayout.PlayerIconHeight,
                Renderable = figureIcons.FindCaterpillarIcon(seatColor),
                VerticalAlignment = VerticalAlignment.Center,
            };

            return caterpillarImage;
        }

        internal static Panel CreateScoreCircle(VolumeTextures volumeTextures, int score)
        {
            var scoreCircle = new Panel
            {
                Width = MatchLayout.ScoreCircleSize,
                Height = MatchLayout.ScoreCircleSize,
                Background = volumeTextures.CircleBrush,
                VerticalAlignment = VerticalAlignment.Center,
            };
            scoreCircle.Widgets.Add(new Label
            {
                Text = DisplayFormats.FormatInteger(score),
                Font = Fonts.Small,
                TextColor = Theme.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });

            return scoreCircle;
        }
    }
}
