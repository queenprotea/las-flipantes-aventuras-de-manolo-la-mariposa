using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal sealed class RecordingSystemParameterRepository : ISystemParameterRepository
    {
        internal Dictionary<string, string> ValuesByCode { get; } = new Dictionary<string, string>();

        public Task<string?> FindValueAsync(string parameterCode)
        {
            ValuesByCode.TryGetValue(parameterCode, out string? parameterValue);
            Task<string?> finding = Task.FromResult(parameterValue);

            return finding;
        }
    }
}
