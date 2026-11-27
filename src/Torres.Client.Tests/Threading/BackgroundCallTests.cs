using System;
using System.Threading.Tasks;

using Torres.Client.Threading;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class BackgroundCallTests
    {
        private const int NegativeSeven = -7;
        private const int Seven = 7;
        private const string NotANumber = "siete";

        [Fact]
        public async Task Test_StartAsync_WithAnOperation_ReturnsItsResult()
        {
            var backgroundCall = new BackgroundCall<int, int>(Math.Abs, NegativeSeven);

            int callResult = await backgroundCall.StartAsync();

            Assert.Equal(Seven, callResult);
        }

        [Fact]
        public async Task Test_StartAsync_WithAFailingOperation_ThrowsItsException()
        {
            var backgroundCall = new BackgroundCall<string, int>(int.Parse, NotANumber);

            await Assert.ThrowsAsync<FormatException>(() => backgroundCall.StartAsync());
        }
    }
}
