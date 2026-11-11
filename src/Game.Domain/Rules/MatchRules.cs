using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public sealed class MatchRules
    {
        public const int RoundCount = 3;

        private const string RoundCountMessage = "The rules must describe exactly three rounds.";

        private readonly IReadOnlyList<RoundLayout> _roundLayouts;

        public MatchRules(IReadOnlyList<RoundLayout> roundLayouts, ActionCosts actionCosts)
        {
            ArgumentNullException.ThrowIfNull(roundLayouts);
            ArgumentNullException.ThrowIfNull(actionCosts);
            if ((roundLayouts.Count != RoundCount) || roundLayouts.Any(roundLayout => roundLayout is null))
            {
                throw new ArgumentException(RoundCountMessage, nameof(roundLayouts));
            }

            _roundLayouts = roundLayouts.ToList();
            ActionCosts = actionCosts;
        }

        public ActionCosts ActionCosts { get; }

        public int GetTurnCount(int roundNumber)
        {
            RoundLayout roundLayout = GetRoundLayout(roundNumber);
            int turnCount = roundLayout.TurnCount;

            return turnCount;
        }

        public int GetBuildCount(int roundNumber, int turnNumber)
        {
            RoundLayout roundLayout = GetRoundLayout(roundNumber);
            ArgumentOutOfRangeException.ThrowIfLessThan(turnNumber, MatchTable.FirstTurnNumber);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(turnNumber, roundLayout.TurnCount);

            int buildCount = roundLayout.BuildCountsByTurn[turnNumber - MatchTable.FirstTurnNumber];

            return buildCount;
        }

        private RoundLayout GetRoundLayout(int roundNumber)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(roundNumber, MatchTable.FirstRoundNumber);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(roundNumber, RoundCount);

            RoundLayout roundLayout = _roundLayouts[roundNumber - MatchTable.FirstRoundNumber];

            return roundLayout;
        }
    }
}
