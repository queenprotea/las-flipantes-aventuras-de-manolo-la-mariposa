using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Checks, while a player fills in the registration form, whether a username or an email can still be used.
    /// </summary>
    [ServiceContract]
    public interface IAvailabilityService
    {
        /// <summary>
        /// Checks whether a username has a valid format and belongs to no account.
        /// </summary>
        /// <param name="username">Username the player is typing.</param>
        /// <returns>Whether the username is available, or why it is not.</returns>
        [OperationContract(Name = "CheckUsernameAvailability")]
        Task<UsernameAvailability> CheckUsernameAvailabilityAsync(string username);

        /// <summary>
        /// Checks whether an email has a valid format and belongs to no account.
        /// </summary>
        /// <param name="email">Email the player is typing.</param>
        /// <returns>Whether the email is available, or why it is not.</returns>
        [OperationContract(Name = "CheckEmailAvailability")]
        Task<EmailAvailability> CheckEmailAvailabilityAsync(string email);
    }
}
