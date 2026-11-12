using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    /// <summary>
    /// Lets the host start a match with the active members of the room, and lets the seated players place their pieces, play their turns or end the match.
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
        /// Places a piece of the caller: the starting caterpillar or the butterfly during the placements, or a new caterpillar on a free flower during the caller's turn, for its action points.
        /// </summary>
        /// <param name="piece">The piece to place, which must be the one the phase asks for.</param>
        /// <param name="destinationCell">The cell where the piece goes.</param>
        /// <returns>Whether the piece was placed, or why not.</returns>
        [OperationContract(Name = "PlacePiece", IsInitiating = false)]
        MatchActionResult PlacePiece(MatchPiece piece, MatchCell destinationCell);

        /// <summary>
        /// Moves a caterpillar of the caller to a neighbor cell during the caller's turn, for the action points that moving and climbing cost; the view lists that cost for each destination.
        /// </summary>
        /// <param name="move">The cell where the caterpillar stands and the cell it goes to.</param>
        /// <returns>Whether the caterpillar moved, or why not.</returns>
        [OperationContract(Name = "MoveCaterpillar", IsInitiating = false)]
        MatchActionResult MoveCaterpillar(MatchMove move);

        /// <summary>
        /// Adds a flower level during the caller's turn: one more level on a flower, or a new flower on the ground next to a single planter.
        /// </summary>
        /// <param name="buildCell">The cell that grows.</param>
        /// <returns>Whether the level was added, or why not.</returns>
        [OperationContract(Name = "AddFlowerLevel", IsInitiating = false)]
        MatchActionResult AddFlowerLevel(MatchCell buildCell);

        /// <summary>
        /// Ends the caller's turn before its time runs out; the action points left are lost.
        /// </summary>
        /// <returns>Whether the turn was ended, or why not.</returns>
        [OperationContract(Name = "EndTurn", IsInitiating = false)]
        MatchActionResult EndTurn();

        /// <summary>
        /// Ends the caller's match for every seated player before it finishes, and takes the room back to waiting.
        /// </summary>
        /// <returns>Whether the match was ended, or why not.</returns>
        [OperationContract(Name = "EndMatch", IsInitiating = false)]
        EndMatchResult EndMatch();
    }
}
