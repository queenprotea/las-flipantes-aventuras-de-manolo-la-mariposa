using System;

using Game.Contracts;

using Torres.Client.Localization;

namespace Torres.Client.Ui
{
    internal static class CardTexts
    {
        internal static string ToNameKey(MatchCard card)
        {
            string nameKey = card switch
            {
                MatchCard.SixActionPoints => TextKeys.Match.SixActionPointsCard,
                MatchCard.SevenActionPoints => TextKeys.Match.SevenActionPointsCard,
                MatchCard.DoubleClimb => TextKeys.Match.DoubleClimbCard,
                MatchCard.GroundLeap => TextKeys.Match.GroundLeapCard,
                MatchCard.FlowerLevelMove => TextKeys.Match.FlowerLevelMoveCard,
                MatchCard.Relocation => TextKeys.Match.RelocationCard,
                MatchCard.PlanterCrossing => TextKeys.Match.PlanterCrossingCard,
                MatchCard.FlowerLevelRemoval => TextKeys.Match.FlowerLevelRemovalCard,
                _ => throw new ArgumentOutOfRangeException(nameof(card)),
            };

            return nameKey;
        }

        internal static string ToEffectKey(MatchCard card)
        {
            string effectKey = card switch
            {
                MatchCard.SixActionPoints => TextKeys.Match.SixActionPointsEffect,
                MatchCard.SevenActionPoints => TextKeys.Match.SevenActionPointsEffect,
                MatchCard.DoubleClimb => TextKeys.Match.DoubleClimbEffect,
                MatchCard.GroundLeap => TextKeys.Match.GroundLeapEffect,
                MatchCard.FlowerLevelMove => TextKeys.Match.FlowerLevelMoveEffect,
                MatchCard.Relocation => TextKeys.Match.RelocationEffect,
                MatchCard.PlanterCrossing => TextKeys.Match.PlanterCrossingEffect,
                MatchCard.FlowerLevelRemoval => TextKeys.Match.FlowerLevelRemovalEffect,
                _ => throw new ArgumentOutOfRangeException(nameof(card)),
            };

            return effectKey;
        }
    }
}
