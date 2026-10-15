using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    [ServiceContract]
    public interface IPasswordRecoveryService
    {
        [OperationContract(Name = "RequestRecoveryCode")]
        Task<RecoveryCodeRequestResult> RequestRecoveryCodeAsync(string email);

        [OperationContract(Name = "VerifyRecoveryCode")]
        Task<RecoveryCodeVerificationResult> VerifyRecoveryCodeAsync(string email, string code);

        [OperationContract(Name = "ResetPassword")]
        Task<PasswordResetResult> ResetPasswordAsync(string email, string code, string newPassword);
    }
}
