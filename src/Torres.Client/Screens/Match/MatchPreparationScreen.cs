using System.Collections.Generic;

using FontStashSharp.RichText;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class MatchPreparationScreen : Screen
    {
        private const double SecondsBeforePlacement = 3;

        private readonly MatchSummaryFlow _matchSummaryFlow;
        private readonly MatchPause _matchPause;
        private readonly LocalizedLabel _summaryLabel = new LocalizedLabel(TextKeys.MatchPreparation.SummaryLabel) { Font = Fonts.Body, TextColor = Theme.MutedInk, TextAlign = TextHorizontalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
        private readonly VerticalStackPanel _seatsPanel = new VerticalStackPanel { Spacing = MatchLayout.PauseRowSpacing, HorizontalAlignment = HorizontalAlignment.Center };

        private double _secondsShown;

        internal MatchPreparationScreen(MatchSummaryFlow matchSummaryFlow, MatchPause matchPause)
            : base(TextKeys.MatchPreparation.HeaderLabel, true)
        {
            _matchSummaryFlow = matchSummaryFlow;
            _matchPause = matchPause;
        }

        internal override void Open()
        {
            _secondsShown = 0;
            MatchView? matchView = _matchSummaryFlow.CurrentMatchView;
            _matchPause.ShowTable(Root, matchView);
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
            _matchPause.Advance(gameTime.ElapsedGameTime);
            _secondsShown += gameTime.ElapsedGameTime.TotalSeconds;
            if (_secondsShown < SecondsBeforePlacement)
            {
                return;
            }

            RequestedScreen = ScreenId.InitialPlacement;
        }

        internal override void RenderScene()
        {
            _matchPause.RenderScene();
        }

        protected override Widget Build()
        {
            var titleLabel = new LocalizedLabel(TextKeys.MatchPreparation.Title)
            {
                Font = Fonts.Heading,
                TextColor = Theme.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var titleBlock = new VerticalStackPanel();
            titleBlock.Widgets.Add(titleLabel);
            titleBlock.Widgets.Add(_summaryLabel);

            var cardContent = new VerticalStackPanel { Spacing = MatchLayout.PauseSectionSpacing };
            cardContent.Widgets.Add(titleBlock);
            cardContent.Widgets.Add(PanelFactory.CreateDivider());
            cardContent.Widgets.Add(new LocalizedLabel(TextKeys.MatchPreparation.SeatingOrderLabel)
            {
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            cardContent.Widgets.Add(_seatsPanel);
            Panel cardLayer = _matchPause.CreateCardLayer(cardContent, MatchLayout.PreparationCardWidth);

            return cardLayer;
        }

        private void ShowSeats(IReadOnlyList<MatchSeat> seats)
        {
            _summaryLabel.TextArguments = new string[] { DisplayFormats.FormatInteger(seats.Count) };
            _seatsPanel.Widgets.Clear();
            foreach (MatchSeat seat in seats)
            {
                HorizontalStackPanel seatPill = _matchPause.CreatePlayerPill(seat);
                seatPill.HorizontalAlignment = HorizontalAlignment.Center;
                _seatsPanel.Widgets.Add(seatPill);
            }
        }
    }
}
