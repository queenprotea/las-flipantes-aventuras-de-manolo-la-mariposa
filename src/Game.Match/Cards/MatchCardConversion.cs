using System;
using System.Collections.Generic;
using System.Linq;

using Game.Contracts;
using Game.Domain;

namespace Game.Match
{
    internal static class MatchCardConversion
    {
        internal static CardPlay ToCardPlay(MatchCardPlay matchCardPlay)
        {
            List<BoardCell> playedCells = matchCardPlay.AffectedCells.Select(MatchCellConversion.ToBoardCell).ToList();
            var cardPlay = new CardPlay(ToActionCard(matchCardPlay.Card), playedCells);

            return cardPlay;
        }

        internal static MatchCard ToMatchCard(ActionCard actionCard)
        {
            MatchCard matchCard = actionCard switch
            {
                ActionCard.SixActionPoints => MatchCard.SixActionPoints,
                ActionCard.SevenActionPoints => MatchCard.SevenActionPoints,
                ActionCard.DoubleClimb => MatchCard.DoubleClimb,
                ActionCard.GroundLeap => MatchCard.GroundLeap,
                ActionCard.FlowerLevelMove => MatchCard.FlowerLevelMove,
                ActionCard.Relocation => MatchCard.Relocation,
                ActionCard.PlanterCrossing => MatchCard.PlanterCrossing,
                ActionCard.FlowerLevelRemoval => MatchCard.FlowerLevelRemoval,
                _ => throw new ArgumentOutOfRangeException(nameof(actionCard)),
            };

            return matchCard;
        }

        private static ActionCard ToActionCard(MatchCard matchCard)
        {
            ActionCard actionCard = matchCard switch
            {
                MatchCard.SixActionPoints => ActionCard.SixActionPoints,
                MatchCard.SevenActionPoints => ActionCard.SevenActionPoints,
                MatchCard.DoubleClimb => ActionCard.DoubleClimb,
                MatchCard.GroundLeap => ActionCard.GroundLeap,
                MatchCard.FlowerLevelMove => ActionCard.FlowerLevelMove,
                MatchCard.Relocation => ActionCard.Relocation,
                MatchCard.PlanterCrossing => ActionCard.PlanterCrossing,
                MatchCard.FlowerLevelRemoval => ActionCard.FlowerLevelRemoval,
                _ => throw new ArgumentOutOfRangeException(nameof(matchCard)),
            };

            return actionCard;
        }
    }
}
