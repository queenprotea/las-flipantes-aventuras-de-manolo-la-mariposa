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
        private const int TagSpacing = 6;
        private const float WithdrawnOpacity = 0.45f;
        private const float FullOpacity = 1f;

        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly MatchPause _matchPause;
        private readonly VerticalStackPanel _playersPanel = new VerticalStackPanel { Spacing = MatchLayout.PauseRowSpacing };
        private readonly CountdownView _countdownView;

        private readonly LocalizedButton _retryButton = CreatePrimaryButton(TextKeys.Disconnection.RetryButton);

        private Task<OpenSessionResult>? _sessionReopening;
        private TimeSpan _sinceLastAttempt;
        private bool _isWaitingForMatch;

        internal DisconnectionScreen(GameSessionConnection gameSessionConnection, PlayerSession playerSession, MatchPause matchPause)
            : base(TextKeys.Disconnection.HeaderLabel, true)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
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
            _sessionReopening = null;
            _isWaitingForMatch = false;
            ClearRequests();
            _countdownView.Restart();
            _matchPause.ShowTable(Root, _playerSession.CurrentMatchView);
            ShowSeats();
            StartReopening();
        }

        internal override void Update(GameTime gameTime)
        {
            _matchPause.Advance(gameTime.ElapsedGameTime);
            _countdownView.Show(FindReconnectionTimeLeft());
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
            cardContent.Widgets.Add(CreateDivider());
            cardContent.Widgets.Add(playersSection);
            cardContent.Widgets.Add(_noticeBox);
            cardContent.Widgets.Add(BuildActions());
            Panel cardLayer = _matchPause.CreateCardLayer(cardContent, MatchLayout.DisconnectionCardWidth);

            return cardLayer;
        }

        private HorizontalStackPanel BuildActions()
        {
            LocalizedButton exitButton = CreateSecondaryButton(TextKeys.Disconnection.ExitMatchButton);
            _matchPause.StyleSecondaryPill(exitButton);
            _matchPause.StylePrimaryPill(_retryButton);
            exitButton.Click += ExitButtonOnClick;
            HorizontalStackPanel actionButtons = CreateButtonRow();
            actionButtons.HorizontalAlignment = HorizontalAlignment.Center;
            actionButtons.Widgets.Add(_retryButton);
            actionButtons.Widgets.Add(exitButton);

            return actionButtons;
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

        private TimeSpan FindReconnectionTimeLeft()
        {
            TimeSpan reconnectionTimeLeft = TimeSpan.Zero;
            if (_playerSession.ReconnectionEndsAtUtc is DateTime reconnectionEndsAtUtc)
            {
                reconnectionTimeLeft = reconnectionEndsAtUtc - DateTime.UtcNow;
            }

            return reconnectionTimeLeft;
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
