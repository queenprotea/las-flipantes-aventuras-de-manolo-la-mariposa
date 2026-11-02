using System.ServiceModel;

namespace Game.Contracts
{
    /// <summary>
    /// Lets the players of a room, with an account or as guests, talk in the single chat of that room.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IChatNoticeReceiver))]
    public interface IChatService
    {
        /// <summary>
        /// Sends a message to every player in the caller's room, the caller included.
        /// </summary>
        /// <param name="text">Text of the message as the player wrote it.</param>
        /// <returns>Whether the message was sent, or why it was rejected.</returns>
        [OperationContract(Name = "SendChatMessage", IsInitiating = false)]
        SendChatMessageResult SendChatMessage(string text);

        /// <summary>
        /// Reads the messages the caller's room still keeps, including those written before the caller arrived.
        /// </summary>
        /// <returns>The kept messages from oldest to newest and the chat limits; no messages when the caller is in no room.</returns>
        [OperationContract(Name = "GetChatHistory", IsInitiating = false)]
        ChatHistoryResponse GetChatHistory();
    }
}
