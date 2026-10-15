using System.ServiceModel;

namespace Torres.ArchitectureSpike.Contracts
{
    public interface IMatchCallback
    {
        [OperationContract(IsOneWay = true)]
        void MatchUpdated(MatchUpdate update);
    }
}
