using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

using Torres.ArchitectureSpike.Contracts;

namespace Torres.ArchitectureSpike.Client
{
    internal static class VictimRun
    {
        private const int VictimExitCode = 0;

        internal static async Task<int> RunAsync(ClientSettings clientSettings, string playerName)
        {
            EndpointAddress serviceAddress = clientSettings.CreateEndpointAddress(ServiceEndpointPaths.GameSession);
            var victimClient = new SpikeClient(playerName, serviceAddress);
            await victimClient.Session.ConnectAsync(playerName);
            await victimClient.Session.JoinMatchAsync();
            await Task.Delay(Timeout.Infinite);

            return VictimExitCode;
        }
    }
}
