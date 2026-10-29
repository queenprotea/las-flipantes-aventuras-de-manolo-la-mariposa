using System.ServiceModel;

namespace Game.Contracts
{
    [ServiceContract]
    public interface ISessionNoticeReceiver
    {
        [OperationContract(Name = "ReceiveSessionTerminated", IsOneWay = true)]
        void ReceiveSessionTerminated(SessionTerminationReason reason);
    }
}
