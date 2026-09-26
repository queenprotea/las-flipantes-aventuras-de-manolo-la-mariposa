using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Lets the client find out whether the server is ready to serve requests.
    /// </summary>
    [ServiceContract]
    public interface IServerStatusService
    {
        /// <summary>
        /// Checks that the server can read its database and reports the result.
        /// </summary>
        [OperationContract(Name = "GetStatus")]
        Task<ServerStatus> GetStatusAsync();
    }
}
