using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface ITurnLayoutRepository
    {
        Task<List<TurnLayout>> GetAllAsync();
    }
}
