using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    [ServiceContract]
    public interface IRankingService
    {
        [OperationContract(Name = "GetGlobalRanking")]
        Task<GlobalRankingResponseContract> GetGlobalRankingAsync(int currentUserId);
    }
}
