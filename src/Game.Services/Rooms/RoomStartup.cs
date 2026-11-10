using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private const string ReportThresholdParameterCode = "report_threshold_occasions";
        private const string LeftoverRoomsClosedMessage = "{0} rooms left open by the previous run were closed.";
        private const string BrokenSanctionLadderMessage = "The sanction levels must be numbered from 1 without gaps and last longer than zero; level {0} breaks the ladder.";
        private const string MissingSanctionLevelsMessage = "The sanction levels are missing.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(RoomStartup));

        private readonly RoomRecordRepository _roomRecordRepository;
        private readonly SanctionLevelRepository _sanctionLevelRepository;
        private readonly SystemParameterReader _systemParameterReader;
        private readonly TimeProvider _timeProvider;

        public RoomStartup(RoomRecordRepository roomRecordRepository, SanctionLevelRepository sanctionLevelRepository, SystemParameterReader systemParameterReader, TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(roomRecordRepository);
            ArgumentNullException.ThrowIfNull(sanctionLevelRepository);
            ArgumentNullException.ThrowIfNull(systemParameterReader);
            ArgumentNullException.ThrowIfNull(timeProvider);

            _roomRecordRepository = roomRecordRepository;
            _sanctionLevelRepository = sanctionLevelRepository;
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

        public async Task<SanctionPolicy> ReadSanctionPolicyAsync()
        {
            int thresholdOccasions = await _systemParameterReader.ReadPositiveAsync(ReportThresholdParameterCode);
            List<SanctionLevel> sanctionLevels = await _sanctionLevelRepository.ListAsync();
            EnsureSanctionLadder(sanctionLevels);
            List<TimeSpan> temporaryBanDurations = sanctionLevels.Select(sanctionLevel => sanctionLevel.BanDuration).ToList();
            var sanctionPolicy = new SanctionPolicy(thresholdOccasions, temporaryBanDurations);

            return sanctionPolicy;
        }

        private static void EnsureSanctionLadder(IReadOnlyList<SanctionLevel> sanctionLevels)
        {
            if (sanctionLevels.Count == 0)
            {
                throw new InvalidOperationException(MissingSanctionLevelsMessage);
            }

            for (var i = 0; i < sanctionLevels.Count; i++)
            {
                SanctionLevel sanctionLevel = sanctionLevels[i];
                if ((sanctionLevel.Level != i + 1) || (sanctionLevel.BanDuration <= TimeSpan.Zero))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, BrokenSanctionLadderMessage, sanctionLevel.Level));
                }
            }
        }
    }
}
