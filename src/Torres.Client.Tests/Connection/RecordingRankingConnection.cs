using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Connection;

namespace Torres.Client.Tests
{
    internal sealed class RecordingRankingConnection : IRankingConnection
    {
        internal Task<GlobalRankingResponse> GetGlobalRankingReply { get; set; } = Task.FromResult(new GlobalRankingResponse());

        internal List<string> CalledMethods { get; } = new List<string>();

        public Task<GlobalRankingResponse> GetGlobalRankingAsync()
        {
            CalledMethods.Add(nameof(GetGlobalRankingAsync));

            return GetGlobalRankingReply;
        }

        public void ReleaseChannel()
        {
            CalledMethods.Add(nameof(ReleaseChannel));
        }
    }
}
