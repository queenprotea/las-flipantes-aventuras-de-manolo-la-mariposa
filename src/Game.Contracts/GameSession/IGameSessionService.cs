using System.ServiceModel;

namespace Game.Contracts
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IGameSessionNoticeReceiver))]
    public interface IGameSessionService : ISessionService, IRoomService, IFriendsService
    {
    }
}
