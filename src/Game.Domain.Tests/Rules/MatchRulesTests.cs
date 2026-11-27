using Xunit;

namespace Game.Domain.Tests
{
    public sealed class MatchRulesTests
    {
        private const int TurnRoundNumberThree = 3;

        [Fact]
        public void Test_GetTurnCount_OfFourTurnRound_ReturnsFour()
        {
            MatchRules rules = SampleRules.TwoSeatRules;

            int turnCount = rules.GetTurnCount(SampleRules.RoundNumberOne);

            Assert.Equal(SampleRules.FourTurns, turnCount);
        }

        [Fact]
        public void Test_GetBuildCount_OfThirdTurnWithThreeSeats_ReturnsTwo()
        {
            MatchRules rules = SampleRules.ThreeSeatRules;

            int buildCount = rules.GetBuildCount(SampleRules.RoundNumberOne, TurnRoundNumberThree);

            Assert.Equal(SampleRules.TwoBuildings, buildCount);
        }
    }
}
