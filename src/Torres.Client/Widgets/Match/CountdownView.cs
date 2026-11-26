using System;

using FontStashSharp;

using Microsoft.Xna.Framework;

using Myra.Graphics2D.UI;

using Torres.Client.UI;

namespace Torres.Client.Widgets
{
    internal sealed class CountdownView
    {
        private readonly FlowerClock _flowerClock;
        private readonly Label _clockLabel;

        private TimeSpan _fullTime;

        internal CountdownView(VolumeTextures volumeTextures, SpriteFontBase clockFont, Color clockColor)
        {
            _flowerClock = new FlowerClock(volumeTextures) { VerticalAlignment = VerticalAlignment.Center };
            _clockLabel = new Label { Font = clockFont, TextColor = clockColor, VerticalAlignment = VerticalAlignment.Center };
            var countdownRow = new HorizontalStackPanel
            {
                Spacing = MatchLayout.HeaderCardSpacing,
                VerticalAlignment = VerticalAlignment.Center,
            };
            countdownRow.Widgets.Add(_flowerClock);
            countdownRow.Widgets.Add(_clockLabel);
            Root = countdownRow;
        }

        internal HorizontalStackPanel Root { get; }

        internal void Restart()
        {
            _fullTime = TimeSpan.Zero;
        }

        internal void Show(TimeSpan timeLeft)
        {
            if (timeLeft > _fullTime)
            {
                _fullTime = timeLeft;
            }

            _clockLabel.Text = DisplayFormats.FormatCountdown(timeLeft);
            _flowerClock.RemainingShare = _fullTime > TimeSpan.Zero ? (float)Math.Max(0d, timeLeft / _fullTime) : 0f;
        }
    }
}
