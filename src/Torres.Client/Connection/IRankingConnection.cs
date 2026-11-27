using System.Threading.Tasks;

using Game.Contracts;

namespace Torres.Client.Connection
{
    internal interface IRankingConnection
    {
        Task<GlobalRankingResponse> GetGlobalRankingAsync();

        void ReleaseChannel();
    }
}
