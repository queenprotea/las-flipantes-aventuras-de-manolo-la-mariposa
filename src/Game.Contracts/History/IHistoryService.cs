using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Reads the finished matches of the caller's account.
    /// </summary>
    [ServiceContract]
    public interface IHistoryService
    {
        /// <summary>
        /// Lists the matches the caller's account took part in.
        /// </summary>
        /// <returns>The caller's matches; empty when the caller has played none.</returns>
        [OperationContract(Name = "GetMatchHistory")]
        Task<MatchHistoryResponse> GetMatchHistoryAsync();

        /// <summary>
        /// Reads the result of one match the caller's account took part in.
        /// </summary>
        /// <param name="matchId">Identifier of the match.</param>
        /// <returns>The match with each player's placement and points, or why it cannot be shown.</returns>
        [OperationContract(Name = "GetMatchDetail")]
        Task<MatchDetailResponse> GetMatchDetailAsync(int matchId);
    }
}
