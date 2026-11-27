using System;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class MatchFlow
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<MatchActionResult> _actionRequest = new PendingRequest<MatchActionResult>();

        internal MatchFlow(GameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal TimeSpan StepTimeLeft => _playerSession.StepTimeLeft;

        internal MatchView? CurrentMatchView => _playerSession.CurrentMatchView;

        internal string RoomCode => _playerSession.CurrentRoom?.Code ?? string.Empty;

        internal bool IsActing => _actionRequest.IsWaiting;

        internal static bool IsViewerPlaying(MatchView matchView)
        {
            bool isViewerPlaying = (matchView.Phase == MatchPhase.PlayerTurn) && (matchView.ActiveSeatNumber == matchView.ViewerSeatNumber);

            return isViewerPlaying;
        }

        internal static ScreenId? FindNextScreen(MatchView matchView)
        {
            ScreenId? nextScreen = matchView.Phase switch
            {
                MatchPhase.ButterflyPlacement => ScreenId.RoundSummary,
                MatchPhase.Finished => ScreenId.Result,
                MatchPhase.CaterpillarPlacement or MatchPhase.PlayerTurn or MatchPhase.AwaitingPlayers => null,
                _ => throw new ArgumentOutOfRangeException(nameof(matchView)),
            };

            return nextScreen;
        }

        internal ScreenId? Open()
        {
            _actionRequest.Forget();
            ScreenId? redirect = _playerSession.CurrentMatchView is null ? ScreenId.MainMenu : null;

            return redirect;
        }

        internal void PlaceCaterpillar(MatchCell targetCell)
        {
            if (!_actionRequest.IsWaiting)
            {
                _actionRequest.Start(_gameSessionConnection.PlacePieceAsync(new PiecePlacement(MatchPiece.Caterpillar, targetCell)));
            }
        }

        internal void AddFlowerLevel(MatchCell targetCell)
        {
            if (!_actionRequest.IsWaiting)
            {
                _actionRequest.Start(_gameSessionConnection.AddFlowerLevelAsync(targetCell));
            }
        }

        internal void MoveCaterpillar(MatchMove move)
        {
            if (!_actionRequest.IsWaiting)
            {
                _actionRequest.Start(_gameSessionConnection.MoveCaterpillarAsync(move));
            }
        }

        internal void DrawCard()
        {
            if (!_actionRequest.IsWaiting)
            {
                _actionRequest.Start(_gameSessionConnection.DrawCardAsync());
            }
        }

        internal void PlayCard(MatchCardPlay cardPlay)
        {
            if (!_actionRequest.IsWaiting)
            {
                _actionRequest.Start(_gameSessionConnection.PlayCardAsync(cardPlay));
            }
        }

        internal void EndTurn()
        {
            if (!_actionRequest.IsWaiting)
            {
                _actionRequest.Start(_gameSessionConnection.EndTurnAsync());
            }
        }

        internal MatchActionOutcome? TakeOutcome()
        {
            FinishedRequest<MatchActionResult>? finishedAction = _actionRequest.TakeFinished();
            MatchActionOutcome? outcome = null;
            if ((finishedAction is not null) && !finishedAction.IsSuccessful)
            {
                outcome = new MatchActionOutcome(Notice.CreateServerError());
            }
            else if ((finishedAction is not null) && (finishedAction.Reply != MatchActionResult.Applied))
            {
                outcome = new MatchActionOutcome(Notice.CreateTitle(ToRejectionKey(finishedAction.Reply)));
            }
            else if (finishedAction is not null)
            {
                outcome = new MatchActionOutcome(null);
            }

            return outcome;
        }

        private static string ToRejectionKey(MatchActionResult actionRejection)
        {
            string rejectionKey = actionRejection switch
            {
                MatchActionResult.NotYourTurn or MatchActionResult.WrongPhase => TextKeys.Match.NotYourTurnNotice,
                MatchActionResult.TimeExpired => TextKeys.Match.TimeExpiredNotice,
                MatchActionResult.NotEnoughActionPoints => TextKeys.Match.NotEnoughPointsNotice,
                MatchActionResult.NoCaterpillarsLeft => TextKeys.Match.NoCaterpillarsLeftNotice,
                MatchActionResult.NotAFlower or MatchActionResult.OutsideBoard => TextKeys.Match.NotAFlowerNotice,
                MatchActionResult.CellOccupied => TextKeys.Match.CellOccupiedNotice,
                MatchActionResult.NotYourCaterpillar => TextKeys.Match.NotYourCaterpillarNotice,
                MatchActionResult.NotNeighborCell => TextKeys.Match.NotNeighborNotice,
                MatchActionResult.ClimbTooHigh => TextKeys.Match.ClimbTooHighNotice,
                MatchActionResult.FlowerAtMaximumLevel => TextKeys.Match.FlowerAtMaximumNotice,
                MatchActionResult.NoBuildsLeft => TextKeys.Match.NoBuildsLeftNotice,
                MatchActionResult.NotNextToPlanter => TextKeys.Match.NotNextToPlanterNotice,
                MatchActionResult.JoinsPlanters => TextKeys.Match.JoinsPlantersNotice,
                MatchActionResult.DeckEmpty => TextKeys.Match.DeckEmptyNotice,
                MatchActionResult.CardNotInHand => TextKeys.Match.CardNotInHandNotice,
                MatchActionResult.PointsCardAlreadyUsed => TextKeys.Match.PointsCardUsedNotice,
                MatchActionResult.WrongCardTarget => TextKeys.Match.WrongCardTargetNotice,
                MatchActionResult.NotInMatch or MatchActionResult.NotIdentified => TextKeys.Common.ServerErrorTitle,
                MatchActionResult.Applied => throw new ArgumentOutOfRangeException(nameof(actionRejection)),
                _ => throw new ArgumentOutOfRangeException(nameof(actionRejection)),
            };

            return rejectionKey;
        }
    }
}
