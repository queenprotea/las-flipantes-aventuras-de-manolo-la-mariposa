namespace Game.Domain
{
    public sealed class ActionCosts
    {
        public ActionCosts(int caterpillarPlacement, int movement, int levelClimb, int flowerLevelAddition, int cardDraw, int cardPlay)
        {
            CaterpillarPlacement = caterpillarPlacement;
            Movement = movement;
            LevelClimb = levelClimb;
            FlowerLevelAddition = flowerLevelAddition;
            CardDraw = cardDraw;
            CardPlay = cardPlay;
        }

        public int CaterpillarPlacement { get; }

        public int Movement { get; }

        public int LevelClimb { get; }

        public int FlowerLevelAddition { get; }

        public int CardDraw { get; }

        public int CardPlay { get; }
    }
}
