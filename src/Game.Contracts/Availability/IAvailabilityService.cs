using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    [ServiceContract]
    public interface IAvailabilityService
    {
        [OperationContract(Name = "CheckUsernameAvailability")]
        Task<UsernameAvailability> CheckUsernameAvailabilityAsync(string username);

        [OperationContract(Name = "CheckEmailAvailability")]
        Task<EmailAvailability> CheckEmailAvailabilityAsync(string email);
    }
}
