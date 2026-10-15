using System.ServiceModel;
using System.Threading.Tasks;

namespace Torres.ArchitectureSpike.Contracts
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ISessionCallback))]
    public interface ISessionOperations
    {
        [OperationContract(Name = "Connect", IsInitiating = true)]
        Task<ConnectStatus> ConnectAsync(string playerName);

        [OperationContract(Name = "Heartbeat", IsInitiating = false)]
        Task<HeartbeatReply> HeartbeatAsync();
    }
}
