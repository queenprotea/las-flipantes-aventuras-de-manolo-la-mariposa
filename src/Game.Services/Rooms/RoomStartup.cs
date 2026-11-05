using System;
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

        private static readonly ILog _log = LogManager.GetLogger(typeof(RoomStartup));

        private readonly RoomRecordRepository _roomRecordRepository;
        private readonly SystemParameterReader _systemParameterReader;
        private readonly TimeProvider _timeProvider;

        public RoomStartup(RoomRecordRepository roomRecordRepository, SystemParameterReader systemParameterReader, TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(roomRecordRepository);
            ArgumentNullException.ThrowIfNull(systemParameterReader);
            ArgumentNullException.ThrowIfNull(timeProvider);

            _roomRecordRepository = roomRecordRepository;
            _systemParameterReader = systemParameterReader;
            _timeProvider = timeProvider;
        }

        public async Task CloseLeftoverRoomsAsync()
        {
            int closedCount = await _roomRecordRepository.CloseAllOpenAsync(_timeProvider.GetUtcNow().UtcDateTime);
            _log.InfoFormat(LeftoverRoomsClosedMessage, closedCount);
        }

        public async Task<TimeSpan> ReadInactivityLimitAsync()
        {
            int inactivityMinutes = await _systemParameterReader.ReadPositiveAsync(InactivityParameterCode);
            TimeSpan inactivityLimit = TimeSpan.FromMinutes(inactivityMinutes);

            return inactivityLimit;
        }

        public async Task<ChatCapacity> ReadChatCapacityAsync()
        {
            int messageMaxLength = await _systemParameterReader.ReadPositiveAsync(MessageMaxLengthParameterCode);
            int messagesKeptPerRoom = await _systemParameterReader.ReadPositiveAsync(MessagesKeptParameterCode);
            var chatCapacity = new ChatCapacity(messageMaxLength, messagesKeptPerRoom);

            return chatCapacity;
        }
    }
}
