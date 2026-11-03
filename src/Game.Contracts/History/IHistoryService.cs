using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Reads the finished matches of the caller's account; guests cannot use it.
    /// </summary>
    [ServiceContract]
    public interface IHistoryService
    {
        /// <summary>
        /// Lists one page of the finished matches the caller's account took part in, from the most recent to the oldest.
        /// </summary>
        /// <param name="pageNumber">Page to read, starting at 1. A number below 1 reads the first page, and a number past the last page reads the last one.</param>
        /// <returns>The matches of the page that was read, its number and how many pages there are; no matches and no pages when the caller has finished none.</returns>
        /// <exception cref="FaultException">The caller is a guest, or the history could not be read.</exception>
        [OperationContract(Name = "GetMatchHistory", IsInitiating = false)]
        Task<MatchHistoryResponse> GetMatchHistoryAsync(int pageNumber);

        /// <summary>
        /// Reads the result of one finished match the caller's account took part in.
        /// </summary>
        /// <param name="matchId">Identifier of the match.</param>
        /// <returns>The match with the placement and points of each player with an account, or why it cannot be shown.</returns>
        /// <exception cref="FaultException">The caller is a guest, or the match could not be read.</exception>
        [OperationContract(Name = "GetMatchDetail", IsInitiating = false)]
        Task<MatchDetailResponse> GetMatchDetailAsync(int matchId);
    }
}
