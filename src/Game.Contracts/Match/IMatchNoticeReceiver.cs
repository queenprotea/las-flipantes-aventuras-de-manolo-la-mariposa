using System.ServiceModel;

namespace Game.Contracts
{
    /// <summary>
    /// Receives the start and the changes of the match the player sits at.
    /// </summary>
    [ServiceContract]
    public interface IMatchNoticeReceiver
    {
        /// <summary>
        /// Announces that the player's room started a match, with the table as the player sees it.
        /// </summary>
        /// <param name="matchView">State of the match as the player sees it when it starts.</param>
        [OperationContract(Name = "ReceiveMatchStarted", IsOneWay = true)]
        void ReceiveMatchStarted(MatchView matchView);

        /// <summary>
        /// Delivers the whole state of the player's match after a change.
        /// </summary>
        /// <param name="matchView">State of the match as the player sees it after the change.</param>
        [OperationContract(Name = "ReceiveMatchStateChanged", IsOneWay = true)]
        void ReceiveMatchStateChanged(MatchView matchView);
    }
}
