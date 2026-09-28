using FontStashSharp.RichText;

using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MatchPreparationScreen : Screen
    {
        private const int SummarySpacing = 12;
        private const int SeatingLabelSpacing = 8;
        private const int SeatRowSpacing = 8;
        private const int SeatColorSize = 20;
        private const double PreviewSecondsBeforePlacement = 3;

        private static readonly Thickness _titleMargin = new Thickness(0, 6, 0, 2);
        private static readonly Thickness _seatRowPadding = new Thickness(0, 4);

        private static readonly Seat[] _previewSeats =
        {
            new Seat("jesus", Theme.MintBrush),
            new Seat("valentin", Theme.LavenderBrush),
            new Seat("salma", Theme.ButterBrush),
            new Seat("scarleth", Theme.SkyBrush),
        };

        private double _secondsShown;

        internal MatchPreparationScreen()
            : base(TextKeys.MatchPreparation.HeaderLabel, true)
        {
        }

        internal override void Open()
        {
            _secondsShown = 0;
        }

        internal override void Update(GameTime gameTime)
        {
            _secondsShown += gameTime.ElapsedGameTime.TotalSeconds;
            if (_secondsShown < PreviewSecondsBeforePlacement)
            {
                return;
            }

            RequestedScreen = ScreenId.InitialPlacement;
        }

        protected override Widget Build()
        {
            Panel scene = Ambience();
            scene.Width = Sizes.PreparationSceneWidth;
            scene.Height = Sizes.PreparationSceneHeight;
            scene.HorizontalAlignment = HorizontalAlignment.Center;
            scene.VerticalAlignment = VerticalAlignment.Top;

            var title = new LocalizedLabel(TextKeys.MatchPreparation.Title)
            {
                Font = Fonts.Heading,
                TextColor = Theme.Ink,
                Margin = _titleMargin,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var summary = new LocalizedLabel(TextKeys.MatchPreparation.SummaryLabel)
            {
                Font = Fonts.Body,
                TextColor = Theme.MutedInk,
                TextAlign = TextHorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextArguments = new object[] { _previewSeats.Length },
            };

            var content = new VerticalStackPanel
            {
                Width = Sizes.MatchPreparationWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            content.Widgets.Add(scene);
            content.Widgets.Add(title);
            content.Widgets.Add(summary);
            content.Widgets.Add(BuildSeatingOrder());

            return content;
        }

        private static VerticalStackPanel BuildSeatingOrder()
        {
            var seating = new VerticalStackPanel
            {
                Spacing = SeatingLabelSpacing,
                Width = Sizes.SeatingOrderWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            seating.Widgets.Add(new LocalizedLabel(TextKeys.MatchPreparation.SeatingOrderLabel)
            {
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
            });

            var seats = new VerticalStackPanel();
            foreach (Seat seat in _previewSeats)
            {
                seats.Widgets.Add(BuildSeatRow(seat));
            }

            seating.Widgets.Add(seats);

            var section = new VerticalStackPanel
            {
                Spacing = SummarySpacing,
                Margin = new Thickness(0, SummarySpacing, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            section.Widgets.Add(Divider());
            section.Widgets.Add(seating);

            return section;
        }

        private static HorizontalStackPanel BuildSeatRow(Seat seat)
        {
            var row = new HorizontalStackPanel
            {
                Spacing = SeatRowSpacing,
                Padding = _seatRowPadding,
            };
            row.Widgets.Add(new Panel
            {
                Width = SeatColorSize,
                Height = SeatColorSize,
                Background = seat.ColorBrush,
                VerticalAlignment = VerticalAlignment.Center,
            });
            row.Widgets.Add(new Label
            {
                Text = seat.PlayerName,
                Font = Fonts.Control,
                TextColor = Theme.Ink,
                VerticalAlignment = VerticalAlignment.Center,
            });

            return row;
        }

        private sealed record Seat(string PlayerName, IBrush ColorBrush);
    }
}
