using System;
using System.Threading.Tasks;

using FontStashSharp.RichText;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class DisconnectionScreen : Screen
    {
        private const int CenterSpacing = 16;
        private const int TagSpacing = 6;
        private const float MutedIconOpacity = 0.5f;
        private const float WithdrawnOpacity = 0.45f;
        private const float FullOpacity = 1f;

        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly VerticalStackPanel _playersPanel = new VerticalStackPanel();
        private readonly Label _clockLabel = new Label
        {
            Font = Fonts.LargeClock,
            TextColor = Theme.BlushInk,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        private readonly LocalizedButton _retryButton = CreatePrimaryButton(TextKeys.Disconnection.RetryButton);

        private Task<OpenSessionResult>? _sessionReopening;
        private TimeSpan _sinceLastAttempt;
        private bool _isWaitingForMatch;

        internal DisconnectionScreen(GameSessionConnection gameSessionConnection, PlayerSession playerSession)
            : base(TextKeys.Disconnection.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(playerSession);

            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            _retryButton.Click += RetryButtonOnClick;
        }

        internal bool WasExitRequested { get; private set; }

        internal bool WasSessionRefused { get; private set; }

        internal bool HasMatchEndedWhileAway { get; private set; }

        internal bool IsMatchAwaitingPlayers { get; private set; }

        internal override void Open()
        {
            _noticeBox.Hide();
            _sessionReopening = null;
            _isWaitingForMatch = false;
            ClearRequests();
            ShowSeats();
            StartReopening();
        }

        internal override void Update(GameTime gameTime)
        {
            _clockLabel.Text = FormatReconnectionTimeLeft();
            FollowReopening();
            bool isWaitingToRetry = (_sessionReopening is null) && !_isWaitingForMatch;
            if (isWaitingToRetry)
            {
                _sinceLastAttempt += gameTime.ElapsedGameTime;
            }

            if (isWaitingToRetry && (_sinceLastAttempt >= _gameSessionConnection.ConnectionCheckPeriod))
            {
                StartReopening();
            }
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
            VerticalStackPanel centerPanel = BuildCenter();
            HorizontalStackPanel columns = CreateColumns();
            columns.Widgets.Add(centerPanel);
            columns.Widgets.Add(BuildSideColumn());
            StackPanel.SetProportionType(centerPanel, ProportionType.Fill);

            return columns;
        }

        private VerticalStackPanel BuildCenter()
        {
            var resumeTurnMessage = new LocalizedLabel(TextKeys.Disconnection.ResumeTurnMessage)
            {
                Width = Sizes.DisconnectionMessageWidth,
                Font = Fonts.Body,
                TextColor = Theme.SoftInk,
                Wrap = true,
                TextAlign = TextHorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            _noticeBox.Width = Sizes.DisconnectionMessageWidth;
            _noticeBox.HorizontalAlignment = HorizontalAlignment.Center;

            var centerPanel = new VerticalStackPanel
            {
                Spacing = CenterSpacing,
                VerticalAlignment = VerticalAlignment.Center,
            };
            centerPanel.Widgets.Add(new Panel
            {
                Width = Sizes.DisconnectionIconSize,
                Height = Sizes.DisconnectionIconSize,
                Background = Theme.MintAltBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Sizes.Border,
                Opacity = MutedIconOpacity,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            centerPanel.Widgets.Add(new LocalizedLabel(TextKeys.Disconnection.ConnectionLostTitle)
            {
                Font = Fonts.Notice,
                TextColor = Theme.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            centerPanel.Widgets.Add(_clockLabel);
            centerPanel.Widgets.Add(resumeTurnMessage);
            centerPanel.Widgets.Add(_noticeBox);
            centerPanel.Widgets.Add(BuildActions());

            return centerPanel;
        }

        private HorizontalStackPanel BuildActions()
        {
            LocalizedButton exitButton = CreateSecondaryButton(TextKeys.Disconnection.ExitMatchButton);
            exitButton.Click += ExitButtonOnClick;

            HorizontalStackPanel actionButtons = CreateButtonRow();
            actionButtons.HorizontalAlignment = HorizontalAlignment.Center;
            actionButtons.Widgets.Add(_retryButton);
            actionButtons.Widgets.Add(exitButton);

            return actionButtons;
        }

        private VerticalStackPanel BuildSideColumn()
        {
            var sideColumn = new VerticalStackPanel
            {
                Width = Sizes.DisconnectionSideWidth,
                VerticalAlignment = VerticalAlignment.Top,
            };
            sideColumn.Widgets.Add(CreateGameSection(TextKeys.Disconnection.PlayersLabel, _playersPanel));

            return sideColumn;
        }

        private void ShowSeats()
        {
            _playersPanel.Widgets.Clear();
            MatchView? matchView = _playerSession.CurrentMatchView;
            if (matchView is not null)
            {
                foreach (MatchSeat seat in matchView.Seats)
                {
                    _playersPanel.Widgets.Add(BuildPlayerRow(seat, seat.SeatNumber == matchView.ViewerSeatNumber));
                }
            }
        }

        private static HorizontalStackPanel BuildPlayerRow(MatchSeat seat, bool isViewer)
        {
            var playerIdentity = new VerticalStackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
            };
            playerIdentity.Widgets.Add(CreatePlayerNameLabel(seat.DisplayName, Theme.Ink));

            HorizontalStackPanel playerTags = BuildTags(seat, isViewer);
            if (playerTags.Widgets.Count > 0)
            {
                playerIdentity.Widgets.Add(playerTags);
            }

            var playerRow = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerRowPadding,
                Opacity = seat.IsRetired ? WithdrawnOpacity : FullOpacity,
            };
            playerRow.Widgets.Add(new Panel
            {
                Width = MatchLayout.PlayerIconSize,
                Height = MatchLayout.PlayerIconSize,
                Background = SeatColors.BrushFor(seat.Color),
                VerticalAlignment = VerticalAlignment.Center,
            });
            playerRow.Widgets.Add(playerIdentity);
            StackPanel.SetProportionType(playerIdentity, ProportionType.Fill);

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
                playerTags.Widgets.Add(CreateTag(TextKeys.Common.YouTag, Theme.MutedInk));
            }

            if (seat.IsGuest)
            {
                playerTags.Widgets.Add(CreateTag(TextKeys.Common.GuestTag, Theme.LavenderInk));
            }

            if (seat.IsRetired)
            {
                playerTags.Widgets.Add(CreateTag(TextKeys.Common.WithdrewTag, Theme.BlushInk));
            }
            else if (isViewer || seat.IsDisconnected)
            {
                playerTags.Widgets.Add(CreateTag(TextKeys.Common.DisconnectedTag, Theme.ButterInk));
            }

            return playerTags;
        }

        private string FormatReconnectionTimeLeft()
        {
            TimeSpan reconnectionTimeLeft = TimeSpan.Zero;
            if (_playerSession.ReconnectionEndsAtUtc is DateTime reconnectionEndsAtUtc)
            {
                reconnectionTimeLeft = reconnectionEndsAtUtc - DateTime.UtcNow;
            }

            string clockText = DisplayFormats.FormatCountdown(reconnectionTimeLeft);

            return clockText;
        }

        private void StartReopening()
        {
            _sinceLastAttempt = TimeSpan.Zero;
            _retryButton.Enabled = false;
            _sessionReopening = _gameSessionConnection.ReopenAsync();
        }

        private void FollowReopening()
        {
            if ((_sessionReopening is null) || !_sessionReopening.IsCompleted)
            {
                return;
            }

            Task<OpenSessionResult> completedReopening = _sessionReopening;
            _sessionReopening = null;
            _retryButton.Enabled = true;
            if (!completedReopening.IsCompletedSuccessfully)
            {
                _gameSessionConnection.AbortSessionChannel();
                ShowServerError();
            }
            else if (completedReopening.Result.Status == OpenSessionStatus.SessionOpened)
            {
                ResumeSession(completedReopening.Result);
            }
            else
            {
                _gameSessionConnection.AbortSessionChannel();
                WasSessionRefused = true;
            }
        }

        private void ResumeSession(OpenSessionResult opening)
        {
            _gameSessionConnection.BeginConnectionChecks(opening);
            _playerSession.RecoverConnection();
            _noticeBox.Hide();
            _retryButton.Enabled = false;
            if (opening.CurrentRoom is not null)
            {
                _playerSession.EnterRoom(opening.CurrentRoom);
            }

            if (opening.CurrentMatch is null)
            {
                LeaveEndedMatch(opening.CurrentRoom);
            }
            else
            {
                FollowSeatedMatch(opening.CurrentRoom, opening.CurrentMatch);
            }
        }

        private void LeaveEndedMatch(Room? currentRoom)
        {
            if (currentRoom is null)
            {
                _playerSession.LeaveRoom();
            }
            else
            {
                _playerSession.LeaveMatch();
            }

            HasMatchEndedWhileAway = true;
        }

        private void FollowSeatedMatch(Room? currentRoom, MatchSeating seatedMatch)
        {
            if (currentRoom is null)
            {
                _playerSession.EnterMatchWithoutRoom(seatedMatch.RoomCode);
            }

            if (seatedMatch.IsAwaitingPlayers)
            {
                IsMatchAwaitingPlayers = true;
            }
            else
            {
                _isWaitingForMatch = true;
            }
        }

        private void ShowServerError()
        {
            string[]? noDetailArguments = null;
            _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, noDetailArguments);
        }

        private void RetryButtonOnClick(object sender, MyraEventArgs e)
        {
            if (_sessionReopening is null)
            {
                StartReopening();
            }
        }

        private void ExitButtonOnClick(object sender, MyraEventArgs e)
        {
            WasExitRequested = true;
        }
    }
}
