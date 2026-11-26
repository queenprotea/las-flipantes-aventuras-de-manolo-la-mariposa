using FontStashSharp.RichText;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class DisconnectionScreen : Screen
    {
        private const int TagSpacing = 6;
        private const float WithdrawnOpacity = 0.45f;
        private const float FullOpacity = 1f;

        private readonly DisconnectionFlow _disconnectionFlow;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly MatchPause _matchPause;
        private readonly VerticalStackPanel _playersPanel = new VerticalStackPanel { Spacing = MatchLayout.PauseRowSpacing };
        private readonly CountdownView _countdownView;

        private readonly LocalizedButton _retryButton = ButtonFactory.CreatePrimaryButton(TextKeys.Disconnection.RetryButton);

        internal DisconnectionScreen(DisconnectionFlow disconnectionFlow, MatchPause matchPause)
            : base(TextKeys.Disconnection.HeaderLabel, true)
        {
            _disconnectionFlow = disconnectionFlow;
            _matchPause = matchPause;
            _countdownView = new CountdownView(matchPause.VolumeTextures, Fonts.Clock, Theme.BlushInk);
            _countdownView.Root.HorizontalAlignment = HorizontalAlignment.Center;
            _retryButton.Click += RetryButtonOnClick;
        }

        internal bool WasExitRequested { get; private set; }

        internal bool WasSessionRefused { get; private set; }

        internal bool HasMatchEndedWhileAway { get; private set; }

        internal bool IsMatchAwaitingPlayers { get; private set; }

        internal override void Open()
        {
            _noticeBox.Hide();
            ClearRequests();
            _countdownView.Restart();
            _matchPause.ShowTable(Root, _disconnectionFlow.CurrentMatchView);
            ShowSeats();
            _disconnectionFlow.Open();
            _retryButton.Enabled = _disconnectionFlow.CanRetry;
        }

        internal override void Update(GameTime gameTime)
        {
            _matchPause.Advance(gameTime.ElapsedGameTime);
            _countdownView.Show(_disconnectionFlow.FindReconnectionTimeLeft());
            _disconnectionFlow.Advance(gameTime.ElapsedGameTime);
            DisconnectionOutcome? outcome = _disconnectionFlow.TakeOutcome();
            if (outcome is not null)
            {
                ShowOutcome(outcome);
            }

            _retryButton.Enabled = _disconnectionFlow.CanRetry;
        }

        internal override void RenderScene()
        {
            _matchPause.RenderScene();
        }

        internal void ClearRequests()
        {
            WasExitRequested = false;
            WasSessionRefused = false;
            HasMatchEndedWhileAway = false;
            IsMatchAwaitingPlayers = false;
        }

        protected override Widget Build()
        {
            var resumeTurnMessage = new LocalizedLabel(TextKeys.Disconnection.ResumeTurnMessage)
            {
                Width = MatchLayout.DisconnectionCardWidth,
                Font = Fonts.Body,
                TextColor = Theme.SoftInk,
                Wrap = true,
                TextAlign = TextHorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            _noticeBox.HorizontalAlignment = HorizontalAlignment.Center;

            var playersSection = new VerticalStackPanel { Spacing = MatchLayout.SectionLabelSpacing };
            playersSection.Widgets.Add(new LocalizedLabel(TextKeys.Disconnection.PlayersLabel) { Font = Fonts.Label, TextColor = Theme.MutedInk });
            playersSection.Widgets.Add(_playersPanel);

            var cardContent = new VerticalStackPanel { Spacing = MatchLayout.PauseSectionSpacing };
            cardContent.Widgets.Add(new LocalizedLabel(TextKeys.Disconnection.ConnectionLostTitle)
            {
                Font = Fonts.Notice,
                TextColor = Theme.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            cardContent.Widgets.Add(_countdownView.Root);
            cardContent.Widgets.Add(resumeTurnMessage);
            cardContent.Widgets.Add(PanelFactory.CreateDivider());
            cardContent.Widgets.Add(playersSection);
            cardContent.Widgets.Add(_noticeBox);
            cardContent.Widgets.Add(BuildActions());
            Panel cardLayer = _matchPause.CreateCardLayer(cardContent, MatchLayout.DisconnectionCardWidth);

            return cardLayer;
        }

        private HorizontalStackPanel BuildActions()
        {
            LocalizedButton exitButton = ButtonFactory.CreateSecondaryButton(TextKeys.Disconnection.ExitMatchButton);
            _matchPause.StyleSecondaryPill(exitButton);
            _matchPause.StylePrimaryPill(_retryButton);
            exitButton.Click += ExitButtonOnClick;
            HorizontalStackPanel actionButtons = PanelFactory.CreateButtonRow();
            actionButtons.HorizontalAlignment = HorizontalAlignment.Center;
            actionButtons.Widgets.Add(_retryButton);
            actionButtons.Widgets.Add(exitButton);

            return actionButtons;
        }

        private void ShowSeats()
        {
            _playersPanel.Widgets.Clear();
            MatchView? matchView = _disconnectionFlow.CurrentMatchView;
            if (matchView is not null)
            {
                foreach (MatchSeat seat in matchView.Seats)
                {
                    _playersPanel.Widgets.Add(BuildPlayerRow(seat, seat.SeatNumber == matchView.ViewerSeatNumber));
                }
            }
        }

        private HorizontalStackPanel BuildPlayerRow(MatchSeat seat, bool isViewer)
        {
            HorizontalStackPanel playerTag = _matchPause.CreatePlayerTag(seat, Fonts.Body);
            HorizontalStackPanel playerRow = _matchPause.CreateStatusPill();
            playerRow.Opacity = seat.IsRetired ? WithdrawnOpacity : FullOpacity;
            playerRow.Widgets.Add(playerTag);
            playerRow.Widgets.Add(BuildTags(seat, isViewer));
            StackPanel.SetProportionType(playerTag, ProportionType.Fill);

            return playerRow;
        }

        private static HorizontalStackPanel BuildTags(MatchSeat seat, bool isViewer)
        {
            var playerTags = new HorizontalStackPanel
            {
                Spacing = TagSpacing,
            };

            if (isViewer)
            {
                playerTags.Widgets.Add(LabelFactory.CreateTag(TextKeys.Common.YouTag, Theme.MutedInk));
            }

            if (seat.IsGuest)
            {
                playerTags.Widgets.Add(LabelFactory.CreateTag(TextKeys.Common.GuestTag, Theme.LavenderInk));
            }

            if (seat.IsRetired)
            {
                playerTags.Widgets.Add(LabelFactory.CreateTag(TextKeys.Common.WithdrewTag, Theme.BlushInk));
            }
            else if (isViewer || seat.IsDisconnected)
            {
                playerTags.Widgets.Add(LabelFactory.CreateTag(TextKeys.Common.DisconnectedTag, Theme.ButterInk));
            }

            return playerTags;
        }

        private void ShowOutcome(DisconnectionOutcome outcome)
        {
            if (outcome.Notice is Notice notice)
            {
                DisplayNotice(_noticeBox, notice);
            }

            if (outcome.IsResumed)
            {
                _noticeBox.Hide();
            }

            WasSessionRefused |= outcome.IsSessionRefused;
            HasMatchEndedWhileAway |= outcome.HasMatchEndedWhileAway;
            IsMatchAwaitingPlayers |= outcome.IsMatchAwaitingPlayers;
        }

        private void RetryButtonOnClick(object sender, MyraEventArgs e)
        {
            _disconnectionFlow.Retry();
        }

        private void ExitButtonOnClick(object sender, MyraEventArgs e)
        {
            WasExitRequested = true;
        }
    }
}
