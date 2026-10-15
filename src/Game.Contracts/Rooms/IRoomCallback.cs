using System.ServiceModel;

namespace Game.Contracts
{
    [ServiceContract]
    public interface IRoomCallback
    {
        [OperationContract(Name = "OnRoomStateChanged", IsOneWay = true)]
        void OnRoomStateChanged(RoomChangeReason reason, RoomContract room);
    }
}
