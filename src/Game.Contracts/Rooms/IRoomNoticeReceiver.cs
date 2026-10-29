using System.ServiceModel;

namespace Game.Contracts
{
    [ServiceContract]
    public interface IRoomNoticeReceiver
    {
        [OperationContract(Name = "ReceiveRoomStateChanged", IsOneWay = true)]
        void ReceiveRoomStateChanged(RoomChangeReason reason, Room room);
    }
}
