using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    [ServiceContract]
    public interface IProfileService
    {
        [OperationContract(Name = "GetProfile")]
        Task<ProfileContract> GetProfileAsync();

        [OperationContract(Name = "CheckProfileUsernameAvailability")]
        Task<UsernameAvailability> CheckProfileUsernameAvailabilityAsync(string username);

        [OperationContract(Name = "UpdateProfile")]
        Task<UpdateProfileResult> UpdateProfileAsync(string username, byte[]? avatarImage);

        [OperationContract(Name = "GetAvatar")]
        Task<AvatarContract> GetAvatarAsync(string avatarReference);
    }
}
