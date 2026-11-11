namespace Game.Domain
{
    public readonly record struct SeatRoundScore(int PlanterPoints, int ButterflyPoints, int ScoredPlanterCount)
    {
        public int TotalPoints => PlanterPoints + ButterflyPoints;
    }
}
