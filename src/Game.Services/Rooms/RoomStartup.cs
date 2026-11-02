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
        private const string MessageMaxLengthParameterCode = "chat_message_max_length";
        private const string MessagesKeptParameterCode = "chat_messages_kept_per_room";
        private const string LeftoverRoomsClosedMessage = "{0} rooms left open by the previous run were closed.";
        private const string MissingParameterMessage = "The system parameter {0} is missing or is not a positive whole number.";

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
            int inactivityMinutes = await ReadPositiveParameterAsync(InactivityParameterCode);
            TimeSpan inactivityLimit = TimeSpan.FromMinutes(inactivityMinutes);

            return inactivityLimit;
        }

        public async Task<ChatLimits> ReadChatLimitsAsync()
        {
            int messageMaxLength = await ReadPositiveParameterAsync(MessageMaxLengthParameterCode);
            int messagesKeptPerRoom = await ReadPositiveParameterAsync(MessagesKeptParameterCode);
            var chatLimits = new ChatLimits(messageMaxLength, messagesKeptPerRoom);

            return chatLimits;
        }

        private async Task<int> ReadPositiveParameterAsync(string parameterCode)
        {
            string? parameterValue = await _systemParameterRepository.FindValueAsync(parameterCode);
            bool isValidValue = int.TryParse(parameterValue, NumberStyles.None, CultureInfo.InvariantCulture, out int positiveValue)
                && (positiveValue > 0);
            if (!isValidValue)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, MissingParameterMessage, parameterCode));
            }

            return positiveValue;
        }
    }
}
