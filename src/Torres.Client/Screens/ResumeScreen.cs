using System;
using System.Linq;
using System.Threading.Tasks;

using FontStashSharp.RichText;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class ResumeScreen : Screen
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly MatchPause _matchPause;
        private readonly VerticalStackPanel _playerRows = new VerticalStackPanel { Spacing = MatchLayout.PauseRowSpacing };
        private readonly CountdownView _countdownView;

        private Task<ReturnToMatchResult>? _returnRequest;
        private MatchView? _shownView;

        internal ResumeScreen(GameSessionConnection gameSessionConnection, PlayerSession playerSession, MatchPause matchPause)
            : base(TextKeys.Resume.HeaderLabel, true)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            _matchPause = matchPause;
            _countdownView = new CountdownView(matchPause.VolumeTextures, Fonts.Clock, Theme.MintInk);
            _countdownView.Root.HorizontalAlignment = HorizontalAlignment.Center;
        }

        internal bool WasMatchLost { get; private set; }

        internal override void Open()
        {
            _noticeBox.Hide();
            _shownView = null;
            WasMatchLost = false;
            _countdownView.Restart();
            _playerRows.Widgets.Clear();
            _matchPause.ShowTable(Root, _playerSession.CurrentMatchView);
            bool hasReturned = (_playerSession.CurrentMatchView is MatchView matchView) && PlayerSession.HasViewerReturned(matchView);
            if (!hasReturned && (_returnRequest is null))
            {
                _returnRequest = _gameSessionConnection.ReturnToMatchAsync();
            }
        }

        internal override void Update(GameTime gameTime)
        {
            _matchPause.Advance(gameTime.ElapsedGameTime);
            _countdownView.Show(FindResumeTimeLeft());
            FollowReturnRequest();
            MatchView? matchView = _playerSession.CurrentMatchView;
            if ((matchView is not null) && !ReferenceEquals(matchView, _shownView) && (matchView.Phase == MatchPhase.AwaitingPlayers))
            {
                _shownView = matchView;
                ShowPlayers(matchView);
            }
        }

        internal override void RenderScene()
        {
            _matchPause.RenderScene();
        }

        internal void ClearRequests()
        {
            WasMatchLost = false;
        }

        protected override Widget Build()
        {
            LocalizedLabel noActionNeeded = CreateCenteredHint(TextKeys.Resume.NoActionNeededMessage);
            LocalizedLabel discardedTurn = CreateCenteredHint(TextKeys.Resume.DiscardedTurnMessage);
            _noticeBox.HorizontalAlignment = HorizontalAlignment.Center;

            var titleBlock = new VerticalStackPanel();
            titleBlock.Widgets.Add(new LocalizedLabel(TextKeys.Resume.Title)
            {
                Font = Fonts.Notice,
                TextColor = Theme.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            titleBlock.Widgets.Add(new LocalizedLabel(TextKeys.Resume.WaitingStatus)
            {
                Font = Fonts.Body,
                TextColor = Theme.MutedInk,
                HorizontalAlignment = HorizontalAlignment.Center,
            });

            var hintBlock = new VerticalStackPanel();
            hintBlock.Widgets.Add(noActionNeeded);
            hintBlock.Widgets.Add(discardedTurn);

            var cardContent = new VerticalStackPanel { Spacing = MatchLayout.PauseSectionSpacing };
            cardContent.Widgets.Add(titleBlock);
            cardContent.Widgets.Add(_countdownView.Root);
            cardContent.Widgets.Add(CreateDivider());
            cardContent.Widgets.Add(_playerRows);
            cardContent.Widgets.Add(hintBlock);
            cardContent.Widgets.Add(_noticeBox);
            Panel cardLayer = _matchPause.CreateCardLayer(cardContent, MatchLayout.ResumeCardWidth);

            return cardLayer;
        }

        private static LocalizedLabel CreateCenteredHint(string textKey)
        {
            LocalizedLabel centeredHint = CreateHint(textKey);
            centeredHint.Width = MatchLayout.ResumeCardWidth;
            centeredHint.Wrap = true;
            centeredHint.TextAlign = TextHorizontalAlignment.Center;
            centeredHint.HorizontalAlignment = HorizontalAlignment.Center;

            return centeredHint;
        }

        private void ShowPlayers(MatchView matchView)
        {
            _playerRows.Widgets.Clear();
            foreach (MatchSeat seat in matchView.Seats.Where(seat => !seat.IsRetired))
            {
                _playerRows.Widgets.Add(BuildPlayerRow(seat));
            }
        }

        private HorizontalStackPanel BuildPlayerRow(MatchSeat seat)
        {
            bool hasReturned = !seat.IsDisconnected;
            HorizontalStackPanel playerTag = _matchPause.CreatePlayerTag(seat, Fonts.Body);
            HorizontalStackPanel playerRow = _matchPause.CreateStatusPill();
            playerRow.Widgets.Add(playerTag);
            playerRow.Widgets.Add(new Label
            {
                Text = hasReturned ? DisplaySymbols.CheckMark : DisplaySymbols.WaitingClock,
                Font = Fonts.Body,
                TextColor = hasReturned ? Theme.MintInk : Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            });
            StackPanel.SetProportionType(playerTag, ProportionType.Fill);

            return playerRow;
        }

        private TimeSpan FindResumeTimeLeft()
        {
            TimeSpan resumeTimeLeft = TimeSpan.Zero;
            bool isWaiting = _playerSession.CurrentMatchView?.Phase == MatchPhase.AwaitingPlayers;
            if (isWaiting && (_playerSession.StepEndsAtUtc is DateTime resumeEndsAtUtc))
            {
                resumeTimeLeft = resumeEndsAtUtc - DateTime.UtcNow;
            }

            return resumeTimeLeft;
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
