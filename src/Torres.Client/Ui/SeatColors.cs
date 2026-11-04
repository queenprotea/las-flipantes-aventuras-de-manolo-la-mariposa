using System;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Graphics2D;

namespace Torres.Client.Ui
{
    internal static class SeatColors
    {
        internal static IBrush BrushFor(SeatColor seatColor)
        {
            IBrush seatBrush = seatColor switch
            {
                SeatColor.Green => Theme.SeatGreenBrush,
                SeatColor.Blue => Theme.SeatBlueBrush,
                SeatColor.Red => Theme.SeatRedBrush,
                SeatColor.Yellow => Theme.SeatYellowBrush,
                _ => throw new ArgumentOutOfRangeException(nameof(seatColor)),
            };

            return seatBrush;
        }

        internal static Color ColorFor(SeatColor seatColor)
        {
            Color sceneColor = seatColor switch
            {
                SeatColor.Green => Theme.SeatGreen,
                SeatColor.Blue => Theme.SeatBlue,
                SeatColor.Red => Theme.SeatRed,
                SeatColor.Yellow => Theme.SeatYellow,
                _ => throw new ArgumentOutOfRangeException(nameof(seatColor)),
            };

            return sceneColor;
        }
    }
}
