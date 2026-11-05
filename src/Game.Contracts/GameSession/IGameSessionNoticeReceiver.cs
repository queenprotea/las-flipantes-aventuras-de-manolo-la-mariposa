using System.ServiceModel;

namespace Game.Contracts
{
    /// <summary>
    /// Receives, on the player's single duplex connection, every notice the server sends without being asked.
    /// </summary>
    [ServiceContract]
    public interface IGameSessionNoticeReceiver : ISessionNoticeReceiver, IRoomNoticeReceiver, IChatNoticeReceiver, IMatchNoticeReceiver
    {
    }
}
