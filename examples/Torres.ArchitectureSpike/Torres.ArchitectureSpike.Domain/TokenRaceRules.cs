using System;

namespace Torres.ArchitectureSpike.Domain
{
    public sealed class TokenRaceRules
    {
        public TokenRaceRules(int goalPosition, int maximumStepsPerTurn)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(goalPosition);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumStepsPerTurn);

            GoalPosition = goalPosition;
            MaximumStepsPerTurn = maximumStepsPerTurn;
        }

        public int GoalPosition { get; }

        public int MaximumStepsPerTurn { get; }
    }
}
