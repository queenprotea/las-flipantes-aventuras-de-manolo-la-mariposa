using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    [ServiceContract]
    public interface IHistoryService
    {
        [OperationContract(Name = "GetMatchHistory")]
        Task<MatchHistoryResponseContract> GetMatchHistoryAsync();

        [OperationContract(Name = "GetMatchDetail")]
        Task<MatchDetailContract> GetMatchDetailAsync(int matchId);
    }
}
