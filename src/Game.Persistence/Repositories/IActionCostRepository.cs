using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface IActionCostRepository
    {
        Task<List<ActionCost>> GetAllAsync();
    }
}
