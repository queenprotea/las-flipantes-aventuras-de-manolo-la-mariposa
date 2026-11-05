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

        internal MatchViewFactory(IReadOnlyList<SeatedEntrant> seatedEntrants)
        {
            _seatedEntrants = seatedEntrants;
        }

        internal MatchView Create(MatchTable table, TimeSpan? placementTimeLeft, int viewerSeatNumber)
        {
            MatchCell butterflyCell = table.ButterflyCell is BoardCell butterflyBoardCell ? MatchCellConversion.ToMatchCell(butterflyBoardCell) : new MatchCell();
            var matchView = new MatchView
            {
                Phase = ToMatchPhase(table.Phase),
                RoundNumber = table.RoundNumber,
                TurnNumber = table.TurnNumber,
                ViewerSeatNumber = viewerSeatNumber,
                ActiveSeatNumber = table.ActiveSeatNumber,
                PlacementTimeLeft = placementTimeLeft,
                Seats = _seatedEntrants.Select(ToMatchSeat).ToList(),
                Flowers = CreateFlowers(table),
                Caterpillars = table.Caterpillars.Select(ToMatchCaterpillar).ToList(),
                HasButterfly = table.ButterflyCell.HasValue,
                ButterflyCell = butterflyCell,
                PlacementTargets = table.FindPlacementTargets().Select(MatchCellConversion.ToMatchCell).ToList(),
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
                _ => throw new ArgumentOutOfRangeException(nameof(phase)),
            };

            return matchPhase;
        }

        private static MatchSeat ToMatchSeat(SeatedEntrant seatedEntrant)
        {
            var matchSeat = new MatchSeat { SeatNumber = seatedEntrant.SeatNumber, DisplayName = seatedEntrant.Entrant.DisplayName, IsGuest = seatedEntrant.Entrant.IsGuest, Color = seatedEntrant.Entrant.Color };

            return matchSeat;
        }

        private static List<MatchFlower> CreateFlowers(MatchTable table)
        {
            IReadOnlyList<Planter> planters = table.FindPlanters();
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
                matchFlowers.Add(new MatchFlower { Cell = MatchCellConversion.ToMatchCell(flower.Cell), Height = flower.Height, PlanterNumber = planterNumber });
            }
        }

        private static MatchCaterpillar ToMatchCaterpillar(Caterpillar caterpillar)
        {
            var matchCaterpillar = new MatchCaterpillar { SeatNumber = caterpillar.SeatNumber, Cell = MatchCellConversion.ToMatchCell(caterpillar.Cell) };

            return matchCaterpillar;
        }
    }
}
