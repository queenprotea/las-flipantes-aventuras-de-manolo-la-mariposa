using System;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class InitialPlacementFlow
    {
        private readonly IGameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<MatchActionResult> _placementRequest = new PendingRequest<MatchActionResult>();

        internal InitialPlacementFlow(IGameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal MatchView? CurrentMatchView => _playerSession.CurrentMatchView;

        internal static bool IsViewerPlacing(MatchView matchView)
        {
            bool isViewerPlacing = (matchView.Phase != MatchPhase.PlayerTurn) && (matchView.ActiveSeatNumber == matchView.ViewerSeatNumber);

            return isViewerPlacing;
        }

        internal static ScreenId? FindPhaseScreen(MatchView matchView)
        {
            ScreenId? phaseScreen = matchView.Phase switch
            {
                MatchPhase.PlayerTurn => ScreenId.Match,
                MatchPhase.Finished => ScreenId.Result,
                MatchPhase.CaterpillarPlacement or MatchPhase.ButterflyPlacement or MatchPhase.AwaitingPlayers => null,
                _ => throw new ArgumentOutOfRangeException(nameof(matchView)),
            };

            return phaseScreen;
        }

        internal ScreenId? Open()
        {
            _placementRequest.Forget();
            ScreenId? redirect = _playerSession.CurrentMatchView is null ? ScreenId.MainMenu : null;

            return redirect;
        }

        internal TimeSpan FindPlacementTimeLeft()
        {
            TimeSpan placementTimeLeft = TimeSpan.Zero;
            if (_playerSession.StepEndsAtUtc is DateTime placementEndsAt)
            {
                placementTimeLeft = placementEndsAt - DateTime.UtcNow;
            }

            return placementTimeLeft;
        }

        internal bool Place(MatchCell destinationCell)
        {
            MatchView? matchView = _playerSession.CurrentMatchView;
            bool canPlace = false;
            if ((matchView is not null) && !_placementRequest.IsWaiting && IsViewerPlacing(matchView))
            {
                canPlace = true;
                MatchPiece placedPiece = matchView.Phase == MatchPhase.ButterflyPlacement ? MatchPiece.Butterfly : MatchPiece.Caterpillar;
                _placementRequest.Start(_gameSessionConnection.PlacePieceAsync(new PiecePlacement(placedPiece, destinationCell)));
            }

            return canPlace;
        }

        internal Notice? TakeNotice()
        {
            FinishedRequest<MatchActionResult>? finishedPlacement = _placementRequest.TakeFinished();
            Notice? notice = null;
            if ((finishedPlacement is not null) && !finishedPlacement.IsSuccessful)
            {
                notice = Notice.CreateServerError();
            }
            else if ((finishedPlacement is not null) && (finishedPlacement.Reply != MatchActionResult.Applied))
            {
                notice = Notice.CreateTitle(ToPlacementRejectionKey(finishedPlacement.Reply));
            }

            return notice;
        }

        private static string ToPlacementRejectionKey(MatchActionResult placementRejection)
        {
            string rejectionKey = placementRejection switch
            {
                MatchActionResult.CellOccupied => TextKeys.InitialPlacement.FlowerOccupiedNotice,
                MatchActionResult.NotAFlower or MatchActionResult.OutsideBoard => TextKeys.InitialPlacement.NotAFlowerNotice,
                MatchActionResult.TimeExpired => TextKeys.InitialPlacement.TimeExpiredNotice,
                MatchActionResult.NotInMatch or MatchActionResult.NotIdentified => TextKeys.Common.ServerErrorTitle,
                MatchActionResult.NotYourTurn or MatchActionResult.WrongPhase => TextKeys.InitialPlacement.NotYourPlacementNotice,
                MatchActionResult.Applied or MatchActionResult.NotEnoughActionPoints or MatchActionResult.NoCaterpillarsLeft => throw new ArgumentOutOfRangeException(nameof(placementRejection)),
                MatchActionResult.NotYourCaterpillar or MatchActionResult.NotNeighborCell or MatchActionResult.ClimbTooHigh => throw new ArgumentOutOfRangeException(nameof(placementRejection)),
                MatchActionResult.FlowerAtMaximumLevel or MatchActionResult.NoBuildsLeft or MatchActionResult.NotNextToPlanter => throw new ArgumentOutOfRangeException(nameof(placementRejection)),
                MatchActionResult.JoinsPlanters or MatchActionResult.DeckEmpty or MatchActionResult.CardNotInHand => throw new ArgumentOutOfRangeException(nameof(placementRejection)),
                MatchActionResult.PointsCardAlreadyUsed or MatchActionResult.WrongCardTarget => throw new ArgumentOutOfRangeException(nameof(placementRejection)),
                _ => throw new ArgumentOutOfRangeException(nameof(placementRejection)),
            };

            return rejectionKey;
        }
    }
}
