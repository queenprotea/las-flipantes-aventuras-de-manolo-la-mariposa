using System.ServiceModel;
using System.Threading.Tasks;

namespace Torres.ArchitectureSpike.Contracts
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IMatchCallback))]
    public interface IMatchOperations
    {
        [OperationContract(Name = "JoinMatch", IsInitiating = false)]
        Task<MatchEntryStatus> JoinMatchAsync();

        [OperationContract(Name = "AdvanceToken", IsInitiating = false)]
        Task<ActionResult> AdvanceTokenAsync(int steps);
    }
}
