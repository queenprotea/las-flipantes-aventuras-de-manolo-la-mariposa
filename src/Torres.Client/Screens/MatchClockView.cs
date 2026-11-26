using System;

using Game.Contracts;

using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MatchClockView
    {
        private readonly LocalizedLabel _roundTurnLabel = new LocalizedLabel(TextKeys.Match.RoundTurnInstruction) { Font = Fonts.Small, TextColor = Theme.Ink };
        private readonly LocalizedLabel _turnStatusLabel = new LocalizedLabel(TextKeys.Match.YourTurnStatus) { Font = Fonts.Small, TextColor = Theme.MintInk };
        private readonly Label _clockLabel = new Label { Font = Fonts.Clock, TextColor = Theme.Ink, VerticalAlignment = VerticalAlignment.Center };
        private readonly FlowerClock _flowerClock;

        internal MatchClockView(VolumeTextures volumeTextures)
        {
            _flowerClock = new FlowerClock(volumeTextures) { VerticalAlignment = VerticalAlignment.Center };
            var turnLines = new VerticalStackPanel { VerticalAlignment = VerticalAlignment.Center };
            turnLines.Widgets.Add(_roundTurnLabel);
            turnLines.Widgets.Add(_turnStatusLabel);
            var clockPill = new HorizontalStackPanel
            {
                Spacing = MatchLayout.HeaderCardSpacing,
                Padding = MatchLayout.HeaderCardPadding,
                Background = volumeTextures.PillBrush,
            };
            clockPill.Widgets.Add(_flowerClock);
            clockPill.Widgets.Add(_clockLabel);
            clockPill.Widgets.Add(turnLines);
            Root = clockPill;
        }

        internal Widget Root { get; }

        internal void Show(MatchView matchView, bool isViewerPlaying)
        {
            MatchSeat activeSeat = matchView.Seats.Find(seat => seat.SeatNumber == matchView.ActiveSeatNumber) ?? matchView.Seats[0];
            _roundTurnLabel.TextArguments = new string[] { DisplayFormats.FormatInteger(matchView.RoundNumber), DisplayFormats.FormatInteger(matchView.TurnRoundNumber), DisplayFormats.FormatInteger(matchView.RoundTurnCount) };
            _turnStatusLabel.TextKey = isViewerPlaying ? TextKeys.Match.YourTurnStatus : TextKeys.Match.OtherPlayerTurnStatus;
            _turnStatusLabel.TextArguments = new string[] { activeSeat.DisplayName };
        }

        internal void ShowClock(TimeSpan stepTimeLeft, float remainingShare)
        {
            _clockLabel.Text = DisplayFormats.FormatCountdown(stepTimeLeft);
            _flowerClock.RemainingShare = remainingShare;
        }
    }
}
