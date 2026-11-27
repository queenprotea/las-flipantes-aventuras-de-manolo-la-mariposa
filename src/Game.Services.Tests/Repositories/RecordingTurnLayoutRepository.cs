using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal sealed class RecordingTurnLayoutRepository : ITurnLayoutRepository
    {
        internal List<TurnLayout> TurnLayouts { get; } = new List<TurnLayout>();

        public Task<List<TurnLayout>> GetAllAsync()
        {
            Task<List<TurnLayout>> reading = Task.FromResult(new List<TurnLayout>(TurnLayouts));

            return reading;
        }
    }
}
