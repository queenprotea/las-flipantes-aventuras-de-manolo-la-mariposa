using System.ServiceModel;

namespace Game.Contracts
{
    /// <summary>
    /// Receives the notices that end the player's game session.
    /// </summary>
    [ServiceContract]
    public interface ISessionNoticeReceiver
    {
        /// <summary>
        /// Tells the player that the server ended their game session.
        /// </summary>
        /// <param name="reason">Why the session was ended.</param>
        [OperationContract(Name = "ReceiveSessionTerminated", IsOneWay = true)]
        void ReceiveSessionTerminated(SessionTerminationReason reason);
    }
}
