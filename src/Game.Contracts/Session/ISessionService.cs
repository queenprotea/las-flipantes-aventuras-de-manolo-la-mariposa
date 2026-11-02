using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Opens, keeps alive and closes the player's game session on the duplex connection.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(ISessionNoticeReceiver))]
    public interface ISessionService
    {
        /// <summary>
        /// Opens the game session of the player the token identifies, ending any older session of the same player.
        /// </summary>
        /// <param name="sessionToken">Token issued at login or when entering as a guest.</param>
        /// <returns>The player's identity, the room they were in and the connection check period, or why the token was rejected.</returns>
        [OperationContract(Name = "OpenSession", IsInitiating = true)]
        OpenSessionResult OpenSession(string sessionToken);

        /// <summary>
        /// Tells the server that the player is still connected.
        /// </summary>
        /// <returns>The server's current time.</returns>
        [OperationContract(Name = "IsAlive", IsInitiating = false)]
        ConnectionCheckResponse IsAlive();

        /// <summary>
        /// Closes the game session; the player leaves the room they were in.
        /// </summary>
        /// <returns>Confirmation that the session was closed.</returns>
        [OperationContract(Name = "CloseSession", IsInitiating = false, IsTerminating = true)]
        Task<CloseSessionResult> CloseSessionAsync();
    }
}
