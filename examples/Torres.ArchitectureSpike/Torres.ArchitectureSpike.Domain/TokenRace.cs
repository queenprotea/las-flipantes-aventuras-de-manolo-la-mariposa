using System;

namespace Torres.ArchitectureSpike.Domain
{
    public sealed class TokenRace
    {
        public const int FirstSeat = 1;
        public const int SeatCount = 2;
        public const int NoWinner = 0;

        private const int MinimumSteps = 1;

        private readonly TokenRaceRules _rules;

        public TokenRace(TokenRaceRules rules)
        {
            ArgumentNullException.ThrowIfNull(rules);

            _rules = rules;
            SeatInTurn = FirstSeat;
        }

        public int TokenPosition { get; private set; }

        public int SeatInTurn { get; private set; }

        public int WinnerSeat { get; private set; }

        public int GoalPosition => _rules.GoalPosition;

        public bool IsFinished => WinnerSeat != NoWinner;

        public StepOutcome Advance(int seat, int steps)
        {
            StepOutcome stepOutcome = ValidateStep(seat, steps);
            if (stepOutcome == StepOutcome.Applied)
            {
                MoveToken(seat, steps);
            }

            return stepOutcome;
        }

        public void ExpireTurn()
        {
            if (IsFinished)
            {
                return;
            }

            PassTurn();
        }

        private StepOutcome ValidateStep(int seat, int steps)
        {
            StepOutcome stepOutcome = StepOutcome.Applied;
            if (IsFinished)
            {
                stepOutcome = StepOutcome.RaceFinished;
            }
            else if (seat != SeatInTurn)
            {
                stepOutcome = StepOutcome.NotYourTurn;
            }
            else if ((steps < MinimumSteps) || (steps > _rules.MaximumStepsPerTurn))
            {
                stepOutcome = StepOutcome.InvalidStepCount;
            }

            return stepOutcome;
        }

        private void MoveToken(int seat, int steps)
        {
            TokenPosition = Math.Min(TokenPosition + steps, _rules.GoalPosition);
            if (TokenPosition == _rules.GoalPosition)
            {
                WinnerSeat = seat;
            }
            else
            {
                PassTurn();
            }
        }

        private void PassTurn()
        {
            SeatInTurn = (SeatInTurn % SeatCount) + FirstSeat;
        }
    }
}
