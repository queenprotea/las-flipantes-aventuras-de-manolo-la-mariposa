using System;

using Game.Contracts;

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
    }
}
