using System.ServiceModel;

namespace Game.Contracts
{
    /// <summary>
    /// Receives the changes of the room the player is in.
    /// </summary>
    [ServiceContract]
    public interface IRoomNoticeReceiver
    {
        /// <summary>
        /// Delivers the whole state of the player's room after a change.
        /// </summary>
        /// <param name="reason">What changed in the room.</param>
        /// <param name="room">State of the room as the player sees it after the change.</param>
        [OperationContract(Name = "ReceiveRoomStateChanged", IsOneWay = true)]
        void ReceiveRoomStateChanged(RoomChangeReason reason, Room room);
    }
}
