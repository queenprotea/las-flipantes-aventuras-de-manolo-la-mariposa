using System;

using Game.Contracts;
using Game.Domain;

namespace Game.Match
{
    internal static class MatchActionResultConversion
    {
        internal static MatchActionResult ToActionResult(PlacementOutcome placementOutcome)
        {
            MatchActionResult actionResult = placementOutcome switch
            {
                PlacementOutcome.Placed => MatchActionResult.Applied,
                PlacementOutcome.WrongPhase => MatchActionResult.WrongPhase,
                PlacementOutcome.NotYourPlacement => MatchActionResult.NotYourTurn,
                PlacementOutcome.NotAFlower => MatchActionResult.NotAFlower,
                PlacementOutcome.FlowerOccupied => MatchActionResult.CellOccupied,
                _ => throw new ArgumentOutOfRangeException(nameof(placementOutcome)),
            };

            return actionResult;
        }

        internal static MatchActionResult ToActionResult(TurnActionOutcome turnActionOutcome)
        {
            MatchActionResult actionResult = turnActionOutcome switch
            {
                TurnActionOutcome.Applied => MatchActionResult.Applied,
                TurnActionOutcome.WrongPhase => MatchActionResult.WrongPhase,
                TurnActionOutcome.NotYourTurn => MatchActionResult.NotYourTurn,
                TurnActionOutcome.NotEnoughActionPoints => MatchActionResult.NotEnoughActionPoints,
                TurnActionOutcome.NoCaterpillarsLeft => MatchActionResult.NoCaterpillarsLeft,
                TurnActionOutcome.NotAFlower => MatchActionResult.NotAFlower,
                TurnActionOutcome.CellOccupied => MatchActionResult.CellOccupied,
                TurnActionOutcome.NotYourCaterpillar => MatchActionResult.NotYourCaterpillar,
                TurnActionOutcome.NotNeighborCell => MatchActionResult.NotNeighborCell,
                TurnActionOutcome.ClimbTooHigh => MatchActionResult.ClimbTooHigh,
                TurnActionOutcome.FlowerAtMaximumLevel => MatchActionResult.FlowerAtMaximumLevel,
                TurnActionOutcome.NoBuildsLeft => MatchActionResult.NoBuildsLeft,
                TurnActionOutcome.NotNextToPlanter => MatchActionResult.NotNextToPlanter,
                TurnActionOutcome.JoinsPlanters => MatchActionResult.JoinsPlanters,
                _ => throw new ArgumentOutOfRangeException(nameof(turnActionOutcome)),
            };

            return actionResult;
        }
    }
}
