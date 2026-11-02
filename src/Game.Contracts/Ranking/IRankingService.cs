using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Reads the global ranking of the players with an account.
    /// </summary>
    [ServiceContract]
    public interface IRankingService
    {
        /// <summary>
        /// Reads the global ranking, from the player with the most wins down.
        /// </summary>
        /// <returns>The ranked players.</returns>
        /// <exception cref="FaultException">The ranking could not be read.</exception>
        [OperationContract(Name = "GetGlobalRanking")]
        Task<GlobalRankingResponse> GetGlobalRankingAsync();
    }
}
