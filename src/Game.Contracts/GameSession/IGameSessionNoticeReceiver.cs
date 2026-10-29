using System.ServiceModel;

namespace Game.Contracts
{
    [ServiceContract]
    public interface IGameSessionNoticeReceiver : ISessionNoticeReceiver, IRoomNoticeReceiver
    {
    }
}
