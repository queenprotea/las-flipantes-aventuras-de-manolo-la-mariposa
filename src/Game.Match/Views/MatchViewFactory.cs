using System;
using System.Collections.Generic;
using System.Linq;

using Game.Contracts;
using Game.Domain;

namespace Game.Match
{
    internal sealed class MatchViewFactory
    {
        private readonly IReadOnlyList<SeatedEntrant> _seatedEntrants;
        private readonly IReadOnlyCollection<int> _disconnectedSeatNumbers;
        private readonly TimeSpan _reconnectionTime;

        internal MatchViewFactory(IReadOnlyList<SeatedEntrant> seatedEntrants, IReadOnlyCollection<int> disconnectedSeatNumbers, TimeSpan reconnectionTime)
        {
            _seatedEntrants = seatedEntrants;
            _disconnectedSeatNumbers = disconnectedSeatNumbers;
            _reconnectionTime = reconnectionTime;
        }

        internal MatchView CreateForSeat(MatchTable matchTable, TimeSpan? stepTimeLeft, int viewerSeatNumber)
        {
            MatchCell butterflyCell = matchTable.ButterflyCell is BoardCell butterflyBoardCell ? MatchCellConversion.ToMatchCell(butterflyBoardCell) : new MatchCell();
            SeatCards viewerCards = matchTable.CardTable.SeatCards[viewerSeatNumber - MatchTable.FirstSeatNumber];
            var matchView = new MatchView
            {
                Phase = ToMatchPhase(matchTable.Phase),
                RoundNumber = matchTable.RoundNumber,
                TurnRoundNumber = matchTable.TurnRoundNumber,
                RoundTurnCount = matchTable.RoundTurnCount,
                RoundButterflyPoints = RoundScoreCalculator.GetButterflyPoints(matchTable.RoundNumber),
                ViewerSeatNumber = viewerSeatNumber,
                ActiveSeatNumber = matchTable.ActiveSeatNumber,
                StepTimeLeft = stepTimeLeft,
                ReconnectionTime = _reconnectionTime,
                ActionPointsLeft = matchTable.ActionPointsLeft,
                BuildsLeft = matchTable.BuildsLeft,
                CaterpillarsPerSeat = MatchTable.CaterpillarsPerSeat,
                ActionCosts = ToMatchActionCosts(matchTable.ActionCosts),
                Seats = _seatedEntrants.Select(seatedEntrant => ToMatchSeat(seatedEntrant, matchTable)).ToList(),
                Flowers = CreateFlowers(matchTable),
                Caterpillars = matchTable.Caterpillars.Select(ToMatchCaterpillar).ToList(),
                HasButterfly = matchTable.ButterflyCell.HasValue,
                ButterflyCell = butterflyCell,
                PlacementTargets = matchTable.FindPlacementTargets().Select(MatchCellConversion.ToMatchCell).ToList(),
                MoveTargets = CreateMoveTargets(matchTable, viewerSeatNumber),
                BuildTargets = matchTable.FindBuildTargets().Select(MatchCellConversion.ToMatchCell).ToList(),
                RankedSeatNumbers = CreateRankedSeatNumbers(matchTable),
                WasAbandoned = matchTable.FinishCause == FinishCause.TooFewSeats,
                Hand = viewerCards.Hand.Select(MatchCardConversion.ToMatchCard).ToList(),
                DeckCount = viewerCards.Deck.Count,
                PlayableCards = matchTable.CardTable.FindPlayableCards(viewerSeatNumber).Select(MatchCardConversion.ToMatchCard).ToList(),
                CanDrawCard = matchTable.CardTable.CanDrawCard(viewerSeatNumber),
                LevelRemovalTargets = matchTable.CardTable.FindLevelRemovalTargets().Select(MatchCellConversion.ToMatchCell).ToList(),
            };

            return matchView;
        }

        private static MatchPhase ToMatchPhase(TablePhase phase)
        {
            MatchPhase matchPhase = phase switch
            {
                TablePhase.CaterpillarPlacement => MatchPhase.CaterpillarPlacement,
                TablePhase.ButterflyPlacement => MatchPhase.ButterflyPlacement,
                TablePhase.PlayerTurn => MatchPhase.PlayerTurn,
                TablePhase.Finished => MatchPhase.Finished,
                _ => throw new ArgumentOutOfRangeException(nameof(phase)),
            };

            return matchPhase;
        }

        private MatchSeat ToMatchSeat(SeatedEntrant seatedEntrant, MatchTable matchTable)
        {
            SeatStanding standing = matchTable.Standings[seatedEntrant.SeatNumber - MatchTable.FirstSeatNumber];
            var matchSeat = new MatchSeat
            {
                SeatNumber = seatedEntrant.SeatNumber,
                DisplayName = seatedEntrant.Entrant.DisplayName,
                IsGuest = seatedEntrant.Entrant.IsGuest,
                Color = seatedEntrant.Entrant.SeatColor,
                CaterpillarsLeft = matchTable.CountCaterpillarsLeft(seatedEntrant.SeatNumber),
                RoundScores = standing.RoundScores.Select(ToMatchRoundScore).ToList(),
                TotalScore = standing.TotalScore,
                IsRetired = standing.IsRetired,
                IsDisconnected = _disconnectedSeatNumbers.Contains(seatedEntrant.SeatNumber),
                HandCount = matchTable.CardTable.SeatCards[seatedEntrant.SeatNumber - MatchTable.FirstSeatNumber].Hand.Count,
                CardPoints = standing.CardPoints,
                FinalScore = standing.FinalScore,
            };

            return matchSeat;
        }

        private static MatchActionCosts ToMatchActionCosts(ActionCosts actionCosts)
        {
            var matchActionCosts = new MatchActionCosts { CaterpillarPlacement = actionCosts.CaterpillarPlacement, Movement = actionCosts.Movement, LevelClimb = actionCosts.LevelClimb, FlowerLevelAddition = actionCosts.FlowerLevelAddition, CardDraw = actionCosts.CardDraw, CardPlay = actionCosts.CardPlay };

            return matchActionCosts;
        }

        private static MatchRoundScore ToMatchRoundScore(SeatRoundScore roundScore)
        {
            var matchRoundScore = new MatchRoundScore { PlanterPoints = roundScore.PlanterPoints, ButterflyPoints = roundScore.ButterflyPoints };

            return matchRoundScore;
        }

        private static List<MatchMoveTarget> CreateMoveTargets(MatchTable matchTable, int viewerSeatNumber)
        {
            var moveTargets = new List<MatchMoveTarget>();
            bool isViewerPlaying = (matchTable.Phase == TablePhase.PlayerTurn) && (matchTable.ActiveSeatNumber == viewerSeatNumber);
            if (isViewerPlaying)
            {
                List<Caterpillar> viewerCaterpillars = matchTable.Caterpillars.Where(caterpillar => caterpillar.SeatNumber == viewerSeatNumber).ToList();
                foreach (Caterpillar viewerCaterpillar in viewerCaterpillars)
                {
                    moveTargets.AddRange(matchTable.FindMoveTargets(viewerCaterpillar.Cell).Select(moveTarget => ToMatchMoveTarget(viewerCaterpillar.Cell, moveTarget)));
                }
            }

            return moveTargets;
        }

        private static MatchMoveTarget ToMatchMoveTarget(BoardCell originCell, MoveTarget moveTarget)
        {
            var matchMoveTarget = new MatchMoveTarget { Origin = MatchCellConversion.ToMatchCell(originCell), Destination = MatchCellConversion.ToMatchCell(moveTarget.Destination), ActionPointCost = moveTarget.ActionPointCost };

            return matchMoveTarget;
        }

        private static List<int> CreateRankedSeatNumbers(MatchTable matchTable)
        {
            var rankedSeatNumbers = new List<int>();
            if (matchTable.Phase == TablePhase.Finished)
            {
                rankedSeatNumbers.AddRange(matchTable.FindRanking().Select(standing => standing.SeatNumber));
            }

            return rankedSeatNumbers;
        }

        private static List<MatchFlower> CreateFlowers(MatchTable matchTable)
        {
            IReadOnlyList<Planter> planters = matchTable.FindPlanters();
            var matchFlowers = new List<MatchFlower>();
            for (var i = 0; i < planters.Count; i++)
            {
                AddPlanterFlowers(planters[i], i + 1, matchFlowers);
            }

            return matchFlowers;
        }

        private static void AddPlanterFlowers(Planter planter, int planterNumber, List<MatchFlower> matchFlowers)
        {
            foreach (Flower flower in planter.Flowers)
            {
                matchFlowers.Add(new MatchFlower { Cell = MatchCellConversion.ToMatchCell(flower.Cell), Level = flower.Level, PlanterNumber = planterNumber });
            }
        }

        private static MatchCaterpillar ToMatchCaterpillar(Caterpillar caterpillar)
        {
            var matchCaterpillar = new MatchCaterpillar { SeatNumber = caterpillar.SeatNumber, Cell = MatchCellConversion.ToMatchCell(caterpillar.Cell) };

            return matchCaterpillar;
        }
    }
}
