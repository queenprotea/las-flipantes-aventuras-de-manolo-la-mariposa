using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Lets a player invite friends to the room they are in, and answer the invitations they receive.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IInvitationCallback))]
    public interface IInvitationService
    {
        /// <summary>
        /// Lists the room invitations the caller received and has not answered.
        /// </summary>
        /// <returns>The caller's pending invitations.</returns>
        [OperationContract(Name = "GetPendingInvitations")]
        Task<PendingInvitationsResponse> GetPendingInvitationsAsync();

        /// <summary>
        /// Invites a friend to the room the caller is in.
        /// </summary>
        /// <param name="targetPlayerId">Identifier of the friend to invite.</param>
        /// <returns>Whether the invitation was sent, or why it was not.</returns>
        [OperationContract(Name = "SendInvitation")]
        Task<SendInvitationResult> SendInvitationAsync(int targetPlayerId);

        /// <summary>
        /// Accepts or rejects a room invitation; accepting it enters the caller into the room.
        /// </summary>
        /// <param name="invitationId">Identifier of the invitation.</param>
        /// <param name="accept">True to accept the invitation and enter the room; false to reject it.</param>
        /// <returns>The room entered, or why the invitation could not be answered.</returns>
        [OperationContract(Name = "RespondInvitation")]
        Task<RespondInvitationResult> RespondInvitationAsync(int invitationId, bool accept);
    }
    
    /// <summary>
    /// Receives the room invitations sent to the player.
    /// </summary>
    [ServiceContract]
    public interface IInvitationCallback
    {
        /// <summary>
        /// Delivers an invitation just sent to the player.
        /// </summary>
        /// <param name="invitation">Invitation with its sender.</param>
        [OperationContract(Name = "OnRoomInvitationReceived", IsOneWay = true)]
        void OnRoomInvitationReceived(RoomInvitation invitation);
    }
}
