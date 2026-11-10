using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Lets a player with an account invite other accounts to the room they are in, and answer the room invitations they received; guests cannot use it.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IInvitationNoticeReceiver))]
    public interface IInvitationService
    {
        /// <summary>
        /// Lists the room invitations the caller's account received and has not answered, including those that arrived while the caller was not connected, from the most recent to the oldest.
        /// </summary>
        /// <returns>The caller's pending invitations; an empty list when there are none.</returns>
        /// <exception cref="FaultException">The caller is a guest, or the invitations could not be read.</exception>
        [OperationContract(Name = "GetPendingInvitations", IsInitiating = false)]
        Task<PendingInvitationsResponse> GetPendingInvitationsAsync();

        /// <summary>
        /// Lists the accounts that have a pending invitation to the room the caller is in, whoever of its members sent it.
        /// </summary>
        /// <returns>The identifiers of the invited accounts; an empty list when nobody is invited or the caller is not in a room.</returns>
        /// <exception cref="FaultException">The caller is a guest, or the invitations could not be read.</exception>
        [OperationContract(Name = "GetRoomInvitees", IsInitiating = false)]
        Task<RoomInviteesResponse> GetRoomInviteesAsync();

        /// <summary>
        /// Invites an account to the room the caller is in, telling it inside the game. The invitation waits in the invitee's inbox, even while the invitee is not connected, until it is answered or the room closes.
        /// </summary>
        /// <param name="targetPlayerId">Identifier of the account to invite; it does not need to be a friend of the caller.</param>
        /// <returns>Whether the invitation was sent, or why it was not.</returns>
        /// <exception cref="FaultException">The caller is a guest.</exception>
        [OperationContract(Name = "SendInvitation", IsInitiating = false)]
        Task<SendInvitationResult> SendInvitationAsync(int targetPlayerId);

        /// <summary>
        /// Answers one of the caller's pending room invitations. Accepting it enters the caller into the room; rejecting it removes it without telling the sender. An invitation that could not be accepted because of the room, such as a full room, stays pending.
        /// </summary>
        /// <param name="roomId">Identifier of the room the invitation is for.</param>
        /// <param name="accept">True to accept the invitation and enter the room; false to reject it.</param>
        /// <returns>How the invitation was answered and, when it was accepted, the room entered; or why it could not be answered.</returns>
        /// <exception cref="FaultException">The caller is a guest.</exception>
        [OperationContract(Name = "RespondInvitation", IsInitiating = false)]
        Task<RespondInvitationResult> RespondInvitationAsync(int roomId, bool accept);
    }
}
