using System.Threading.Tasks;

using Game.Contracts;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class ServerStatusServiceTests
    {
        private readonly ServerStatusService _statusService = new ServerStatusService(new RecordingPlayerRepository());

        [Fact]
        public async Task Test_GetStatusAsync_WithTheDatabaseAvailable_ReturnsReady()
        {
            ServerStatus status = await _statusService.GetStatusAsync();

            Assert.Equal(ServerStatus.Ready, status);
        }
    }
}
