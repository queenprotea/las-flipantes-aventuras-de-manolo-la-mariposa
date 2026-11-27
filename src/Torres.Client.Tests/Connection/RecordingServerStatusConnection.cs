using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Connection;

namespace Torres.Client.Tests
{
    internal sealed class RecordingServerStatusConnection : IServerStatusConnection
    {
        internal Task<ServerStatus> GetStatusReply { get; set; } = Task.FromResult(default(ServerStatus));

        internal List<string> CalledMethods { get; } = new List<string>();

        public Task<ServerStatus> GetStatusAsync()
        {
            CalledMethods.Add(nameof(GetStatusAsync));

            return GetStatusReply;
        }

        public void ReleaseChannel()
        {
            CalledMethods.Add(nameof(ReleaseChannel));
        }
    }
}
