using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal sealed class RecordingActionCostRepository : IActionCostRepository
    {
        internal List<ActionCost> ActionCosts { get; } = new List<ActionCost>();

        public Task<List<ActionCost>> GetAllAsync()
        {
            Task<List<ActionCost>> reading = Task.FromResult(new List<ActionCost>(ActionCosts));

            return reading;
        }
    }
}
