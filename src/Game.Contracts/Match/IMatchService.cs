using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Lets the host start a match with the active members of the room, and lets the seated players place their pieces or end the match.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IMatchNoticeReceiver))]
    public interface IMatchService
    {
        /// <summary>
        /// Starts a match with the active members of the caller's room, seated by their order of arrival.
        /// </summary>
        /// <returns>Whether the match started, or why not, with the banned player and the end of the ban when one of them is banned.</returns>
        [OperationContract(Name = "StartMatch", IsInitiating = false)]
        Task<StartMatchResult> StartMatchAsync();

        /// <summary>
        /// Places the caller's starting caterpillar on a flower, while it is the caller's placement.
        /// </summary>
        /// <param name="row">Row of the cell, from 0 to 7.</param>
        /// <param name="column">Column of the cell, from 0 to 7.</param>
        /// <returns>Whether the caterpillar was placed, or why not.</returns>
        [OperationContract(Name = "PlaceCaterpillar", IsInitiating = false)]
        PlacementResult PlaceCaterpillar(int row, int column);

        /// <summary>
        /// Places the butterfly on a flower, while the caller is the player who places it.
        /// </summary>
        /// <param name="row">Row of the cell, from 0 to 7.</param>
        /// <param name="column">Column of the cell, from 0 to 7.</param>
        /// <returns>Whether the butterfly was placed, or why not.</returns>
        [OperationContract(Name = "PlaceButterfly", IsInitiating = false)]
        PlacementResult PlaceButterfly(int row, int column);

        /// <summary>
        /// Ends the caller's match for every seated player and takes the room back to waiting, while there is still no way to finish or give up a match.
        /// </summary>
        /// <returns>Whether the match was ended, or why not.</returns>
        [OperationContract(Name = "EndMatch", IsInitiating = false)]
        EndMatchResult EndMatch();
    }
}
