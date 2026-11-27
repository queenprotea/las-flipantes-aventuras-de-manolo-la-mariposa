using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal sealed class RecordingBuildAllowanceRepository : IBuildAllowanceRepository
    {
        internal List<BuildAllowance> BuildAllowances { get; } = new List<BuildAllowance>();

        public Task<List<BuildAllowance>> GetAllAsync()
        {
            Task<List<BuildAllowance>> reading = Task.FromResult(new List<BuildAllowance>(BuildAllowances));

            return reading;
        }
    }
}
