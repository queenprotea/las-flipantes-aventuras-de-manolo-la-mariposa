using System.ServiceModel;

namespace Game.Contracts
{
    /// <summary>
    /// Groups, on the player's single duplex connection, every operation that needs to know who the player is: the session, the rooms, the friends, the room invitations, the chat, the match history, the match itself and the profile.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IGameSessionNoticeReceiver))]
    public interface IGameSessionService : ISessionService, IRoomService, IFriendsService, IInvitationService, IChatService, IHistoryService, IMatchService, IProfileService
    {
    }
}
