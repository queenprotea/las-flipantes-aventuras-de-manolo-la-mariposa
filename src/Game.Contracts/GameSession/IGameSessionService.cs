using System.ServiceModel;

namespace Game.Contracts
{
    /// <summary>
    /// Groups, on the player's single duplex connection, every operation that needs to know who the player is: the session, the rooms, the friends, the chat and the match history.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IGameSessionNoticeReceiver))]
    public interface IGameSessionService : ISessionService, IRoomService, IFriendsService, IChatService, IHistoryService
    {
    }
}
