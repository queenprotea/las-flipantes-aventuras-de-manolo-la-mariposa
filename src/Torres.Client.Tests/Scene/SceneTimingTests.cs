using System;

using Microsoft.Xna.Framework;

using Torres.Client.Scene;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class SceneTimingTests
    {
        private const int Precision = 4;
        private const double PeriodSeconds = 4d;
        private const double DurationSeconds = 2d;
        private const float MiddleWave = 0.5f;
        private const float HighestWave = 1f;
        private const float LowestSignedWave = -1f;
        private const float ArcHeight = 2f;
        private const float HalfTravel = 0.5f;
        private const float NoTravel = 0f;
        private const float NoLift = 0f;
        private const float NoProgress = 0f;
        private const float HalfProgress = 0.5f;
        private const float FullProgress = 1f;

        private static readonly TimeSpan _cycleStart = TimeSpan.Zero;
        private static readonly TimeSpan _quarterCycle = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan _halfCycle = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan _threeQuartersCycle = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan _afterTheDuration = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan _beforeTheStart = TimeSpan.FromSeconds(-1);

        [Fact]
        public void Test_FindCycleWave_AtTheStartOfTheCycle_ReturnsTheMiddle()
        {
            float cycleWave = SceneTiming.FindCycleWave(_cycleStart, PeriodSeconds);

            Assert.Equal(MiddleWave, cycleWave, Precision);
        }

        [Fact]
        public void Test_FindCycleWave_AtAQuarterOfTheCycle_ReturnsTheTop()
        {
            float cycleWave = SceneTiming.FindCycleWave(_quarterCycle, PeriodSeconds);

            Assert.Equal(HighestWave, cycleWave, Precision);
        }

        [Fact]
        public void Test_FindSignedCycleWave_AtThreeQuartersOfTheCycle_ReturnsTheBottom()
        {
            float signedWave = SceneTiming.FindSignedCycleWave(_threeQuartersCycle, PeriodSeconds);

            Assert.Equal(LowestSignedWave, signedWave, Precision);
        }

        [Fact]
        public void Test_FindCycleRadians_AtHalfTheCycle_ReturnsPi()
        {
            float cycleRadians = SceneTiming.FindCycleRadians(_halfCycle, PeriodSeconds);

            Assert.Equal(MathHelper.Pi, cycleRadians, Precision);
        }

        [Fact]
        public void Test_FindArcLift_HalfwayThroughTheTravel_ReturnsTheArcHeight()
        {
            float arcLift = SceneTiming.FindArcLift(ArcHeight, HalfTravel);

            Assert.Equal(ArcHeight, arcLift, Precision);
        }

        [Fact]
        public void Test_FindArcLift_AtTheStartOfTheTravel_ReturnsNoLift()
        {
            float arcLift = SceneTiming.FindArcLift(ArcHeight, NoTravel);

            Assert.Equal(NoLift, arcLift, Precision);
        }

        [Fact]
        public void Test_FindProgress_BeforeTheStart_ReturnsNoProgress()
        {
            float progress = SceneTiming.FindProgress(_beforeTheStart, _cycleStart, DurationSeconds);

            Assert.Equal(NoProgress, progress, Precision);
        }

        [Fact]
        public void Test_FindProgress_HalfwayThroughTheDuration_ReturnsHalf()
        {
            float progress = SceneTiming.FindProgress(_quarterCycle, _cycleStart, DurationSeconds);

            Assert.Equal(HalfProgress, progress, Precision);
        }

        [Fact]
        public void Test_FindProgress_AfterTheDuration_ReturnsFullProgress()
        {
            float progress = SceneTiming.FindProgress(_afterTheDuration, _cycleStart, DurationSeconds);

            Assert.Equal(FullProgress, progress, Precision);
        }

        [Fact]
        public void Test_EaseOutWithOvershoot_AtFullProgress_ReturnsFullProgress()
        {
            float easedProgress = SceneTiming.EaseOutWithOvershoot(FullProgress);

            Assert.Equal(FullProgress, easedProgress, Precision);
        }

        [Fact]
        public void Test_EaseOutWithOvershoot_WithoutProgress_ReturnsNoProgress()
        {
            float easedProgress = SceneTiming.EaseOutWithOvershoot(NoProgress);

            Assert.Equal(NoProgress, easedProgress, Precision);
        }
    }
}
