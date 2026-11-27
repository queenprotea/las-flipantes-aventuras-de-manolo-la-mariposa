using System.Threading.Tasks;

using Game.Contracts;

namespace Torres.Client.Connection
{
    internal interface IServerStatusConnection
    {
        Task<ServerStatus> GetStatusAsync();

        void ReleaseChannel();
    }
}
