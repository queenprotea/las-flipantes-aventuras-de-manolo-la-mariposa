using System.Collections.Concurrent;
using System.Threading.Tasks;

using Torres.Client.Threading;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class BackgroundWorkTests
    {
        private const int Seven = 7;

        private readonly ConcurrentQueue<int> _receivedValues = new ConcurrentQueue<int>();

        [Fact]
        public async Task Test_StartAsync_WithAWork_RunsItWithTheArgument()
        {
            var backgroundWork = new BackgroundWork<int>(_receivedValues.Enqueue, Seven);

            await backgroundWork.StartAsync();

            Assert.Contains(Seven, _receivedValues);
        }
    }
}
