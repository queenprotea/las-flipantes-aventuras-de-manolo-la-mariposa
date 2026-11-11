using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public sealed class RoundLayout
    {
        private const string NoTurnsMessage = "A round must have at least one turn.";
        private const string NegativeBuildCountMessage = "A turn cannot hand out a negative number of buildings.";

        public RoundLayout(IReadOnlyList<int> buildCountsByTurn)
        {
            ArgumentNullException.ThrowIfNull(buildCountsByTurn);
            if (buildCountsByTurn.Count == 0)
            {
                throw new ArgumentException(NoTurnsMessage, nameof(buildCountsByTurn));
            }

            if (buildCountsByTurn.Any(buildCount => buildCount < 0))
            {
                throw new ArgumentException(NegativeBuildCountMessage, nameof(buildCountsByTurn));
            }

            BuildCountsByTurn = buildCountsByTurn.ToList();
        }

        public IReadOnlyList<int> BuildCountsByTurn { get; }

        public int TurnCount => BuildCountsByTurn.Count;
    }
}
