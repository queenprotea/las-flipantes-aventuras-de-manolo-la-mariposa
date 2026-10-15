using System.ServiceModel;

namespace Torres.ArchitectureSpike.Contracts
{
    public interface ISessionCallback
    {
        [OperationContract(IsOneWay = true)]
        void PlayerConnectionChanged(PlayerConnectionNotice notice);
    }
}
