using System.ServiceModel;

namespace Game.Contracts
{
    /// <summary>
    /// Receives the chat messages of the room the player is in, as soon as they are sent.
    /// </summary>
    [ServiceContract]
    public interface IChatNoticeReceiver
    {
        /// <summary>
        /// Delivers a message just sent to the player's room, including the player's own.
        /// </summary>
        /// <param name="message">Message as every player of the room sees it.</param>
        [OperationContract(Name = "ReceiveChatMessage", IsOneWay = true)]
        void ReceiveChatMessage(ChatMessage message);
    }
}
