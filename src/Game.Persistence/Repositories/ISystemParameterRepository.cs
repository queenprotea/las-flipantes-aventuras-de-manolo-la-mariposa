using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface ISystemParameterRepository
    {
        Task<string?> FindValueAsync(string parameterCode);
    }
}
