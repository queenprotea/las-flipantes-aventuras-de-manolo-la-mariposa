using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    [ServiceContract(CallbackContract = typeof(ISessionNoticeReceiver))]
    public interface ISessionService
    {
        [OperationContract(Name = "OpenSession", IsInitiating = true)]
        OpenSessionResult OpenSession(string sessionToken);

        [OperationContract(Name = "IsAlive", IsInitiating = false)]
        ConnectionCheckResponse IsAlive();

        [OperationContract(Name = "CloseSession", IsInitiating = false, IsTerminating = true)]
        Task<CloseSessionResult> CloseSessionAsync();
    }
}
