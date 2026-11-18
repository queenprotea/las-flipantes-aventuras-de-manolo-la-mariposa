using System;
using System.Linq;
using System.Threading.Tasks;

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
    internal sealed class ResumeScreen : Screen
    {
        private const int TitleTopSpacing = 4;
        private const int StatusBottomSpacing = 14;
        private const int PlayersTopSpacing = 22;
        private const int PlayersTopPadding = 16;
        private const int HintTopSpacing = 16;

        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly VerticalStackPanel _playerRows = new VerticalStackPanel();
        private readonly Label _clockLabel = new Label
        {
            Font = Fonts.Clock,
            TextColor = Theme.MintInk,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        private Task<ReturnToMatchResult>? _returnRequest;
        private MatchView? _shownView;

        internal ResumeScreen(GameSessionConnection gameSessionConnection, PlayerSession playerSession)
            : base(TextKeys.Resume.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(playerSession);

            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal bool WasMatchLost { get; private set; }

        internal override void Open()
        {
            _noticeBox.Hide();
            _shownView = null;
            WasMatchLost = false;
            _playerRows.Widgets.Clear();
            bool hasReturned = (_playerSession.CurrentMatchView is MatchView matchView) && PlayerSession.HasViewerReturned(matchView);
            if (!hasReturned && (_returnRequest is null))
            {
                _returnRequest = _gameSessionConnection.ReturnToMatchAsync();
            }
        }

        internal override void Update(GameTime gameTime)
        {
            _clockLabel.Text = FormatResumeTimeLeft();
            FollowReturnRequest();
            MatchView? matchView = _playerSession.CurrentMatchView;
            if ((matchView is not null) && !ReferenceEquals(matchView, _shownView) && (matchView.Phase == MatchPhase.AwaitingPlayers))
            {
                _shownView = matchView;
                ShowPlayers(matchView);
            }
        }

        internal void ClearRequests()
        {
            WasMatchLost = false;
        }

        protected override Widget Build()
        {
            LocalizedLabel noActionNeeded = CreateCenteredHint(TextKeys.Resume.NoActionNeededMessage);
            noActionNeeded.Margin = new Thickness(0, HintTopSpacing, 0, 0);
            LocalizedLabel discardedTurn = CreateCenteredHint(TextKeys.Resume.DiscardedTurnMessage);
            _noticeBox.HorizontalAlignment = HorizontalAlignment.Center;

            var screenLayout = new VerticalStackPanel
            {
                Width = Sizes.ResumeWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            screenLayout.Widgets.Add(new Panel
            {
                Width = Sizes.ResumeIconSize,
                Height = Sizes.ResumeIconSize,
                Background = Theme.MintAltBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            screenLayout.Widgets.Add(new LocalizedLabel(TextKeys.Resume.Title)
            {
                Font = Fonts.Notice,
                TextColor = Theme.Ink,
                Margin = new Thickness(0, TitleTopSpacing, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            screenLayout.Widgets.Add(new LocalizedLabel(TextKeys.Resume.WaitingStatus)
            {
                Font = Fonts.Body,
                TextColor = Theme.MutedInk,
                Margin = new Thickness(0, 0, 0, StatusBottomSpacing),
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            screenLayout.Widgets.Add(_clockLabel);
            screenLayout.Widgets.Add(BuildPlayers());
            screenLayout.Widgets.Add(noActionNeeded);
            screenLayout.Widgets.Add(discardedTurn);
            screenLayout.Widgets.Add(_noticeBox);

            return screenLayout;
        }

        private static LocalizedLabel CreateCenteredHint(string textKey)
        {
            LocalizedLabel centeredHint = CreateHint(textKey);
            centeredHint.TextAlign = TextHorizontalAlignment.Center;
            centeredHint.HorizontalAlignment = HorizontalAlignment.Center;

            return centeredHint;
        }

        private VerticalStackPanel BuildPlayers()
        {
            _playerRows.Margin = new Thickness(0, PlayersTopPadding, 0, 0);

            var playersPanel = new VerticalStackPanel
            {
                Margin = new Thickness(0, PlayersTopSpacing, 0, 0),
            };
            playersPanel.Widgets.Add(CreateDivider());
            playersPanel.Widgets.Add(_playerRows);

            return playersPanel;
        }

        private void ShowPlayers(MatchView matchView)
        {
            _playerRows.Widgets.Clear();
            foreach (MatchSeat seat in matchView.Seats.Where(seat => !seat.IsRetired))
            {
                _playerRows.Widgets.Add(BuildPlayerRow(seat));
            }
        }

        private static HorizontalStackPanel BuildPlayerRow(MatchSeat seat)
        {
            bool hasReturned = !seat.IsDisconnected;
            Label nameLabel = CreatePlayerNameLabel(seat.DisplayName, Theme.Ink);
            var playerRow = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerRowPadding,
            };
            playerRow.Widgets.Add(new Panel
            {
                Width = MatchLayout.PlayerIconSize,
                Height = MatchLayout.PlayerIconSize,
                Background = SeatColors.BrushFor(seat.Color),
                VerticalAlignment = VerticalAlignment.Center,
            });
            playerRow.Widgets.Add(nameLabel);
            playerRow.Widgets.Add(new Label
            {
                Text = hasReturned ? DisplaySymbols.CheckMark : DisplaySymbols.WaitingClock,
                Font = Fonts.Small,
                TextColor = hasReturned ? Theme.MintInk : Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            });
            StackPanel.SetProportionType(nameLabel, ProportionType.Fill);

            return playerRow;
        }

        private string FormatResumeTimeLeft()
        {
            TimeSpan resumeTimeLeft = TimeSpan.Zero;
            bool isWaiting = _playerSession.CurrentMatchView?.Phase == MatchPhase.AwaitingPlayers;
            if (isWaiting && (_playerSession.StepEndsAtUtc is DateTime resumeEndsAtUtc))
            {
                resumeTimeLeft = resumeEndsAtUtc - DateTime.UtcNow;
            }

            string clockText = DisplayFormats.FormatCountdown(resumeTimeLeft);

            return clockText;
        }

        private void FollowReturnRequest()
        {
            if ((_returnRequest is null) || !_returnRequest.IsCompleted)
            {
                return;
            }

            Task<ReturnToMatchResult> completedReturn = _returnRequest;
            _returnRequest = null;
            if (!completedReturn.IsCompletedSuccessfully)
            {
                string[]? noDetailArguments = null;
                _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, noDetailArguments);
            }
            else if (completedReturn.Result != ReturnToMatchResult.Returned)
            {
                WasMatchLost = true;
            }
        }
    }
}
