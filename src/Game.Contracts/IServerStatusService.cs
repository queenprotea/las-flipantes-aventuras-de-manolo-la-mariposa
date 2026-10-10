using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    [ServiceContract]
    public interface IServerStatusService
    {
        [OperationContract(Name = "GetStatus")]
        Task<ServerStatus> GetStatusAsync();
    }
}
