using System;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class MaintenanceTimingTests
    {
        [Fact]
        public void Test_Constructor_WithoutPeriod_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MaintenanceTiming(TimeSpan.Zero));
        }
    }
}
