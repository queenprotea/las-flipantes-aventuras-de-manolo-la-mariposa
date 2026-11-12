using System;

using Xunit;

namespace Game.Domain.Tests
{
    public sealed class MatchRulesTests
    {
        private const int RoundNumberOne = 1;
        private const int RoundNumberFour = 4;
        private const int TurnRoundNumberThree = 3;
        private const int TurnNumberFive = 5;
        private const int FourTurns = 4;
        private const int NegativeBuildCount = -1;
        private const int NegativeCost = -1;

        [Fact]
        public void Test_Constructor_WithTwoRounds_ThrowsArgumentException()
        {
            var twoRounds = new RoundTurnPlan[] { SampleRules.FourTurnRound, SampleRules.FourTurnRound };

            Assert.Throws<ArgumentException>(() => new MatchRules(twoRounds, SampleRules.Costs));
        }

        [Fact]
        public void Test_GetTurnCount_OfFourTurnRound_ReturnsFour()
        {
            MatchRules rules = SampleRules.TwoSeatRules;

            int turnCount = rules.GetTurnCount(RoundNumberOne);

            Assert.Equal(FourTurns, turnCount);
        }

        [Fact]
        public void Test_GetBuildCount_OfThirdTurnWithThreeSeats_ReturnsTwo()
        {
            MatchRules rules = SampleRules.ThreeSeatRules;

            int buildCount = rules.GetBuildCount(RoundNumberOne, TurnRoundNumberThree);

            Assert.Equal(SampleRules.TwoBuildings, buildCount);
        }

        [Fact]
        public void Test_GetBuildCount_OfTurnBeyondTheRound_ThrowsArgumentOutOfRangeException()
        {
            MatchRules rules = SampleRules.TwoSeatRules;

            Assert.Throws<ArgumentOutOfRangeException>(() => rules.GetBuildCount(RoundNumberOne, TurnNumberFive));
        }

        [Fact]
        public void Test_GetTurnCount_OfRoundFour_ThrowsArgumentOutOfRangeException()
        {
            MatchRules rules = SampleRules.TwoSeatRules;

            Assert.Throws<ArgumentOutOfRangeException>(() => rules.GetTurnCount(RoundNumberFour));
        }

        [Fact]
        public void Test_RoundLayoutConstructor_WithoutTurns_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new RoundTurnPlan(Array.Empty<int>()));
        }

        [Fact]
        public void Test_RoundLayoutConstructor_WithNegativeBuildCount_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new RoundTurnPlan(new int[] { NegativeBuildCount }));
        }

        [Fact]
        public void Test_ActionCostsConstructor_WithNegativeCost_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ActionCosts(NegativeCost, SampleRules.MovementCost, SampleRules.ClimbCost, SampleRules.BuildingCost));
        }
    }
}
