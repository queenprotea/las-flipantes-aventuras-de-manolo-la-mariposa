using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Creates accounts and identifies players, with their account or as guests, so they can open a game session.
    /// </summary>
    [ServiceContract]
    public interface IAccountService
    {
        /// <summary>
        /// Creates an account with the given username, email and password.
        /// </summary>
        /// <param name="username">Name the player chooses, unique among accounts.</param>
        /// <param name="email">Email address of the account, unique among accounts.</param>
        /// <param name="password">Password as the player typed it.</param>
        /// <returns>Whether the account was created, or which rule the registration broke.</returns>
        [OperationContract(Name = "Register")]
        Task<RegistrationResult> RegisterAsync(string username, string email, string password);

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
    }
}
