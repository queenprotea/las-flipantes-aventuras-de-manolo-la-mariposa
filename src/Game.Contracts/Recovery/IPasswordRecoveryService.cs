using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Lets a player who forgot their password set a new one with a code sent to the account's email.
    /// </summary>
    [ServiceContract]
    public interface IPasswordRecoveryService
    {
        /// <summary>
        /// Sends a recovery code to the email of an account.
        /// </summary>
        /// <param name="email">Email of the account.</param>
        /// <returns>Whether the code was sent, or why not.</returns>
        [OperationContract(Name = "RequestRecoveryCode")]
        Task<RecoveryCodeRequestResult> RequestRecoveryCodeAsync(string email);

        /// <summary>
        /// Checks whether a recovery code is the one sent to an email and is still valid.
        /// </summary>
        /// <param name="email">Email the code was sent to.</param>
        /// <param name="code">Code as the player typed it.</param>
        /// <returns>Whether the code is valid.</returns>
        [OperationContract(Name = "VerifyRecoveryCode")]
        Task<RecoveryCodeVerificationResult> VerifyRecoveryCodeAsync(string email, string code);

        /// <summary>
        /// Replaces the password of an account, proving ownership with a recovery code.
        /// </summary>
        /// <param name="email">Email of the account.</param>
        /// <param name="code">Recovery code sent to that email.</param>
        /// <param name="newPassword">New password as the player typed it.</param>
        /// <returns>Whether the password was replaced, or why not.</returns>
        [OperationContract(Name = "ResetPassword")]
        Task<PasswordResetResult> ResetPasswordAsync(string email, string code, string newPassword);
    }
}
