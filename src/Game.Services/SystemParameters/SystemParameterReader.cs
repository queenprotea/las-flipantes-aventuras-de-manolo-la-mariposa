using System;
using System.Globalization;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Services
{
    public sealed class SystemParameterReader
    {
        private const string MissingParameterMessage = "The system parameter {0} is missing or is not a positive whole number.";

        private readonly SystemParameterRepository _systemParameterRepository;

        public SystemParameterReader(SystemParameterRepository systemParameterRepository)
        {
            ArgumentNullException.ThrowIfNull(systemParameterRepository);

            _systemParameterRepository = systemParameterRepository;
        }

        public Task<int> ReadPositiveAsync(string parameterCode)
        {
            ArgumentNullException.ThrowIfNull(parameterCode);

            Task<int> parameterReading = ReadPositiveValueAsync(parameterCode);

            return parameterReading;
        }

        private async Task<int> ReadPositiveValueAsync(string parameterCode)
        {
            string? parameterValue = await _systemParameterRepository.FindValueAsync(parameterCode);
            bool isPositiveNumber = int.TryParse(parameterValue, NumberStyles.None, CultureInfo.InvariantCulture, out int parameterNumber)
                && (parameterNumber > 0);
            if (!isPositiveNumber)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, MissingParameterMessage, parameterCode));
            }

            return parameterNumber;
        }
    }
}
