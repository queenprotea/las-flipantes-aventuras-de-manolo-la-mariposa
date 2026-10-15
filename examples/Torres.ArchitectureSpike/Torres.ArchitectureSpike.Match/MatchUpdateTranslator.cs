using System;

using Torres.ArchitectureSpike.Contracts;
using Torres.ArchitectureSpike.Domain;

namespace Torres.ArchitectureSpike.Match
{
    internal static class MatchUpdateTranslator
    {
        internal static MatchUpdate ToUpdate(TokenRace race, MatchEventCode eventCode, int recipientSeat)
        {
            var update = new MatchUpdate
            {
                EventCode = eventCode,
                TokenPosition = race.TokenPosition,
                GoalPosition = race.GoalPosition,
                SeatInTurn = race.SeatInTurn,
                RecipientSeat = recipientSeat,
                WinnerSeat = race.WinnerSeat,
            };

            return update;
        }

        internal static ActionResult ToActionResult(StepOutcome stepOutcome) => stepOutcome switch
        {
            StepOutcome.Applied => ActionResult.Applied,
            StepOutcome.NotYourTurn => ActionResult.NotYourTurn,
            StepOutcome.InvalidStepCount => ActionResult.InvalidStepCount,
            StepOutcome.RaceFinished => ActionResult.MatchFinished,
            _ => throw new ArgumentOutOfRangeException(nameof(stepOutcome))
        };
    }
}
