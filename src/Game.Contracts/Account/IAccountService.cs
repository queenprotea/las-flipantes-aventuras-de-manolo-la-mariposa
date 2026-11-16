using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Creates accounts, identifies players with their account or as guests so they can open a game session, and lets a player who forgot their password set a new one.
    /// </summary>
    [ServiceContract]
    public interface IAccountService
    {
        /// <summary>
        /// Starts the creation of an account and sends a verification code to the given email. The account does not exist until the code is confirmed.
        /// </summary>
        /// <param name="username">Name the player chooses, unique among accounts.</param>
        /// <param name="email">Email address of the account, unique among accounts; the code is sent to it.</param>
        /// <param name="password">Password as the player typed it.</param>
        /// <returns>Whether the code was sent, or which rule the registration broke.</returns>
        [OperationContract(Name = "RequestRegistrationCode")]
        Task<RegistrationResult> RequestRegistrationCodeAsync(string username, string email, string password);

        /// <summary>
        /// Creates the account whose verification code was sent to the given email and, when it is created, issues the token that opens the game session.
        /// </summary>
        /// <param name="email">Email the verification code was sent to.</param>
        /// <param name="code">Code as the player typed it.</param>
        /// <returns>The identity and session token of the new account, or why it was not created.</returns>
        [OperationContract(Name = "ConfirmRegistration")]
        Task<RegistrationConfirmationResult> ConfirmRegistrationAsync(string email, string code);

        /// <summary>
        /// Checks a player's credentials and, when they match, issues the token that opens the game session.
        /// </summary>
        /// <param name="username">Username of the account.</param>
        /// <param name="password">Password as the player typed it.</param>
        /// <returns>The player's identity and session token, or why the login was rejected.</returns>
        [OperationContract(Name = "Login")]
        Task<LoginResult> LoginAsync(string username, string password);

        /// <summary>
        /// Identifies a player without an account under a free alias, without storing anything about them.
        /// </summary>
        /// <param name="guestAlias">Alias the guest chose; surrounding spaces are ignored.</param>
        /// <returns>The session token of the guest, or why the alias was rejected.</returns>
        [OperationContract(Name = "EnterAsGuest")]
        GuestAccessResult EnterAsGuest(string guestAlias);

        /// <summary>
        /// Sends a recovery code to the email of an account. The answer is the same whether or not the email belongs to an account, so it never reveals which emails are registered.
        /// </summary>
        /// <param name="email">Email of the account.</param>
        /// <returns>Whether the request was accepted, or why not.</returns>
        [OperationContract(Name = "RequestRecoveryCode")]
        Task<RecoveryCodeRequestResult> RequestRecoveryCodeAsync(string email);

        /// <summary>
        /// Checks whether a recovery code is the one sent to an email and can still be used, without using it up.
        /// </summary>
        /// <param name="email">Email the code was sent to.</param>
        /// <param name="code">Code as the player typed it.</param>
        /// <returns>Whether the code is valid.</returns>
        [OperationContract(Name = "VerifyRecoveryCode")]
        Task<RecoveryCodeVerificationResult> VerifyRecoveryCodeAsync(string email, string code);

        /// <summary>
        /// Replaces the password of an account, proving ownership with a recovery code that is used up when the password changes.
        /// </summary>
        /// <param name="email">Email of the account.</param>
        /// <param name="code">Recovery code sent to that email.</param>
        /// <param name="newPassword">New password as the player typed it.</param>
        /// <returns>Whether the password was replaced, or why not.</returns>
        [OperationContract(Name = "ResetPassword")]
        Task<PasswordResetResult> ResetPasswordAsync(string email, string code, string newPassword);
    }
}
