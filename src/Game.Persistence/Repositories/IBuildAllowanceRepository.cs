using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface IBuildAllowanceRepository
    {
        Task<List<BuildAllowance>> GetAllAsync();
    }
}
