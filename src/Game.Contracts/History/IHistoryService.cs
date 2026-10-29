using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    [ServiceContract]
    public interface IHistoryService
    {
        [OperationContract(Name = "GetMatchHistory")]
        Task<MatchHistoryResponse> GetMatchHistoryAsync();

        [OperationContract(Name = "GetMatchDetail")]
        Task<MatchDetailResponse> GetMatchDetailAsync(int matchId);
    }
}
