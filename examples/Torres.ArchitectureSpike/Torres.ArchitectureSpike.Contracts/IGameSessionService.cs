using System.ServiceModel;

namespace Torres.ArchitectureSpike.Contracts
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IGameSessionCallback))]
    public interface IGameSessionService : ISessionOperations, IMatchOperations
    {
    }
}
