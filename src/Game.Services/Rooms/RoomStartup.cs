using System;
using System.Globalization;
using System.Threading.Tasks;

using Game.Persistence;

using log4net;

namespace Game.Services
{
    public sealed class RoomStartup
    {
        private const string InactivityParameterCode = "room_inactivity_minutes";
        private const string LeftoverRoomsClosedMessage = "{0} rooms left open by the previous run were closed.";
        private const string MissingParameterMessage = "The system parameter room_inactivity_minutes is missing or is not a positive number of minutes.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(RoomStartup));

        private readonly RoomRecordRepository _roomRecordRepository;
        private readonly SystemParameterRepository _systemParameterRepository;
        private readonly TimeProvider _timeProvider;

        public RoomStartup(RoomRecordRepository roomRecordRepository, SystemParameterRepository systemParameterRepository, TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(roomRecordRepository);
            ArgumentNullException.ThrowIfNull(systemParameterRepository);
            ArgumentNullException.ThrowIfNull(timeProvider);

            _roomRecordRepository = roomRecordRepository;
            _systemParameterRepository = systemParameterRepository;
            _timeProvider = timeProvider;
        }

        public async Task CloseLeftoverRoomsAsync()
        {
            int closedCount = await _roomRecordRepository.CloseAllOpenAsync(_timeProvider.GetUtcNow().UtcDateTime);
            _log.InfoFormat(LeftoverRoomsClosedMessage, closedCount);
        }

        public async Task<TimeSpan> ReadInactivityLimitAsync()
        {
            string? parameterValue = await _systemParameterRepository.FindValueAsync(InactivityParameterCode);
            bool isValidLimit = int.TryParse(parameterValue, NumberStyles.None, CultureInfo.InvariantCulture, out int inactivityMinutes)
                && (inactivityMinutes > 0);
            if (!isValidLimit)
            {
                throw new InvalidOperationException(MissingParameterMessage);
            }

            TimeSpan inactivityLimit = TimeSpan.FromMinutes(inactivityMinutes);

            return inactivityLimit;
        }
    }
}
