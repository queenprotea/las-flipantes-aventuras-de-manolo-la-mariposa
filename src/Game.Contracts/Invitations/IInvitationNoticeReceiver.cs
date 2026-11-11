using System.ServiceModel;

namespace Game.Contracts
{
    /// <summary>
    /// Receives the room invitations sent to the player while they are connected.
    /// </summary>
    [ServiceContract]
    public interface IInvitationNoticeReceiver
    {
        /// <summary>
        /// Delivers a room invitation just sent to the player's account.
        /// </summary>
        /// <param name="invitation">Invitation as it appears in the player's inbox.</param>
        [OperationContract(Name = "ReceiveRoomInvitation", IsOneWay = true)]
        void ReceiveRoomInvitation(RoomInvitation invitation);
    }
}
