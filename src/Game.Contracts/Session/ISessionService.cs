using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Opens and closes the player's game session on the duplex connection.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(ISessionNoticeReceiver))]
    public interface ISessionService
    {
        /// <summary>
        /// Opens the game session of the player the token identifies, ending any older session of the same player.
        /// </summary>
        /// <param name="sessionToken">Token issued at login or when entering as a guest, or the reconnection token of a match in which the player still holds a seat.</param>
        /// <returns>The player's identity, the room they were in and their seat in a match, or why the token was rejected.</returns>
        [OperationContract(Name = "OpenSession", IsInitiating = true)]
        OpenSessionResult OpenSession(string sessionToken);

        /// <summary>
        /// Closes the game session; the player leaves the room they were in.
        /// </summary>
        /// <returns>Confirmation that the session was closed.</returns>
        [OperationContract(Name = "CloseSession", IsInitiating = false, IsTerminating = true)]
        Task<CloseSessionResult> CloseSessionAsync();
    }
}
