using System;
using System.Collections.Generic;

using FontStashSharp.RichText;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MatchPreparationScreen : Screen
    {
        private const int SummarySpacing = 12;
        private const int SeatingLabelSpacing = 8;
        private const int SeatRowSpacing = 8;
        private const int SeatColorSize = 20;
        private const double SecondsBeforePlacement = 3;

        private static readonly Thickness _titleMargin = new Thickness(0, 6, 0, 2);
        private static readonly Thickness _seatRowPadding = new Thickness(0, 4);

        private readonly PlayerSession _playerSession;
        private readonly LocalizedLabel _summaryLabel = new LocalizedLabel(TextKeys.MatchPreparation.SummaryLabel) { Font = Fonts.Body, TextColor = Theme.MutedInk, TextAlign = TextHorizontalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
        private readonly VerticalStackPanel _seatsPanel = new VerticalStackPanel();

        private double _secondsShown;

        internal MatchPreparationScreen(PlayerSession playerSession)
            : base(TextKeys.MatchPreparation.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(playerSession);

            _playerSession = playerSession;
        }

        internal override void Open()
        {
            _secondsShown = 0;
            MatchView? matchView = _playerSession.CurrentMatchView;
            if (matchView is null)
            {
                RequestedScreen = ScreenId.MainMenu;
            }
            else
            {
                ShowSeats(matchView.Seats);
            }
        }

        internal override void Update(GameTime gameTime)
        {
            _secondsShown += gameTime.ElapsedGameTime.TotalSeconds;
            if (_secondsShown < SecondsBeforePlacement)
            {
                return;
            }

            RequestedScreen = ScreenId.InitialPlacement;
        }

        protected override Widget Build()
        {
            Panel ambiencePanel = CreateAmbiencePanel();
            ambiencePanel.Width = Sizes.PreparationSceneWidth;
            ambiencePanel.Height = Sizes.PreparationSceneHeight;
            ambiencePanel.HorizontalAlignment = HorizontalAlignment.Center;
            ambiencePanel.VerticalAlignment = VerticalAlignment.Top;

            var titleLabel = new LocalizedLabel(TextKeys.MatchPreparation.Title)
            {
                Font = Fonts.Heading,
                TextColor = Theme.Ink,
                Margin = _titleMargin,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var screenLayout = new VerticalStackPanel
            {
                Width = Sizes.MatchPreparationWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            screenLayout.Widgets.Add(ambiencePanel);
            screenLayout.Widgets.Add(titleLabel);
            screenLayout.Widgets.Add(_summaryLabel);
            screenLayout.Widgets.Add(BuildSeatingOrder());

            return screenLayout;
        }

        private VerticalStackPanel BuildSeatingOrder()
        {
            var seatingList = new VerticalStackPanel
            {
                Spacing = SeatingLabelSpacing,
                Width = Sizes.SeatingOrderWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            seatingList.Widgets.Add(new LocalizedLabel(TextKeys.MatchPreparation.SeatingOrderLabel)
            {
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
            });

            seatingList.Widgets.Add(_seatsPanel);

            var seatingOrderSection = new VerticalStackPanel
            {
                Spacing = SummarySpacing,
                Margin = new Thickness(0, SummarySpacing, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            seatingOrderSection.Widgets.Add(CreateDivider());
            seatingOrderSection.Widgets.Add(seatingList);

            return seatingOrderSection;
        }

        private static HorizontalStackPanel BuildSeatRow(MatchSeat seat)
        {
            var seatRow = new HorizontalStackPanel
            {
                Spacing = SeatRowSpacing,
                Padding = _seatRowPadding,
            };
            seatRow.Widgets.Add(new Panel
            {
                Width = SeatColorSize,
                Height = SeatColorSize,
                Background = SeatColors.BrushFor(seat.Color),
                VerticalAlignment = VerticalAlignment.Center,
            });
            seatRow.Widgets.Add(new Label
            {
                Text = seat.DisplayName,
                Font = Fonts.Control,
                TextColor = Theme.Ink,
                VerticalAlignment = VerticalAlignment.Center,
            });

            return seatRow;
        }

        private void ShowSeats(IReadOnlyList<MatchSeat> seats)
        {
            _summaryLabel.TextArguments = new string[] { DisplayFormats.FormatInteger(seats.Count) };
            _seatsPanel.Widgets.Clear();
            foreach (MatchSeat seat in seats)
            {
                _seatsPanel.Widgets.Add(BuildSeatRow(seat));
            }
        }
    }
}
