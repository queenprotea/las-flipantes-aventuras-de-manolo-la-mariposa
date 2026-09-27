using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    [ServiceContract]
    public interface IAccountService
    {
        [OperationContract(Name = "Register")]
        Task<RegistrationResult> RegisterAsync(string username, string email, string password);

        [OperationContract(Name = "Login")]
        Task<LoginResult> LoginAsync(string username, string password);
    }
}
