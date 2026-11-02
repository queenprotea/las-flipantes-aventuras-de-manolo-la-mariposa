using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Tells clients whether the server can serve them.
    /// </summary>
    [ServiceContract]
    public interface IServerStatusService
    {
        /// <summary>
        /// Checks whether the server and its database are ready.
        /// </summary>
        /// <returns>Whether the server is ready, or that its database cannot be used.</returns>
        [OperationContract(Name = "GetStatus")]
        Task<ServerStatus> GetStatusAsync();
    }
}
