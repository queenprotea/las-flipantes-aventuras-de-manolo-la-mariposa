using System;

namespace Game.Domain
{
    public sealed class ActionCosts
    {
        public ActionCosts(int caterpillarPlacement, int movement, int levelClimb, int flowerLevelAddition)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(caterpillarPlacement);
            ArgumentOutOfRangeException.ThrowIfNegative(movement);
            ArgumentOutOfRangeException.ThrowIfNegative(levelClimb);
            ArgumentOutOfRangeException.ThrowIfNegative(flowerLevelAddition);

            CaterpillarPlacement = caterpillarPlacement;
            Movement = movement;
            LevelClimb = levelClimb;
            FlowerLevelAddition = flowerLevelAddition;
        }

        public int CaterpillarPlacement { get; }

        public int Movement { get; }

        public int LevelClimb { get; }

        public int FlowerLevelAddition { get; }
    }
}
