using System;
using System.IO;
using System.Threading.Tasks;

using Game.Services;

using log4net;

using Microsoft.Extensions.DependencyInjection;

using Npgsql;

namespace Game.Server
{
    public sealed class ServerStartup
    {
        private const string AvatarFolderFailedMessage = "The avatar folder could not be created; the server does not start.";
        private const string StartingBoardFailedMessage = "The starting board in the configuration is not valid; the server does not start.";
        private const string StartupDatabaseFailedMessage = "The database could not be used while the server started; the server does not start.";
        private const string StoredConfigurationFailedMessage = "The configuration stored in the database is not valid; the server does not start.";

        private readonly IServiceProvider _serviceProvider;
        private readonly ILog _hostLog;
        private MatchService? _recordingMatchService;
        private MaintenanceSupervisor? _startedSupervisor;
        private TimeSpan _roomInactivityLimit;

        public ServerStartup(IServiceProvider serviceProvider, ILog hostLog)
        {
            _serviceProvider = serviceProvider;
            _hostLog = hostLog;
        }

        public async Task<bool> PrepareAsync()
        {
            bool isPrepared = PrepareAvatarFolder() && CreateMatchService() && await StartFromDatabaseAsync();

            return isPrepared;
        }

        public void StartSupervisor()
        {
            MaintenanceSupervisor maintenanceSupervisor = _serviceProvider.GetRequiredService<MaintenanceSupervisor>();
            maintenanceSupervisor.Start(_roomInactivityLimit);
            _startedSupervisor = maintenanceSupervisor;
        }

        public async Task StopAsync()
        {
            if (_startedSupervisor is not null)
            {
                await _startedSupervisor.StopAsync();
            }

            if (_recordingMatchService is not null)
            {
                await _recordingMatchService.FinishRecordingAsync();
            }
        }

        private bool PrepareAvatarFolder()
        {
            AvatarStore avatarStore = _serviceProvider.GetRequiredService<AvatarStore>();
            bool isPrepared;
            try
            {
                avatarStore.EnsureFolder();
                isPrepared = true;
            }
            catch (IOException exception)
            {
                _hostLog.Fatal(AvatarFolderFailedMessage, exception);
                isPrepared = false;
            }
            catch (UnauthorizedAccessException exception)
            {
                _hostLog.Fatal(AvatarFolderFailedMessage, exception);
                isPrepared = false;
            }

            return isPrepared;
        }

        private bool CreateMatchService()
        {
            bool isCreated;
            try
            {
                _recordingMatchService = _serviceProvider.GetRequiredService<MatchService>();
                isCreated = true;
            }
            catch (ArgumentException exception)
            {
                _hostLog.Fatal(StartingBoardFailedMessage, exception);
                isCreated = false;
            }

            return isCreated;
        }

        private async Task<bool> StartFromDatabaseAsync()
        {
            bool hasStarted;
            try
            {
                await StartStoredServicesAsync();
                hasStarted = true;
            }
            catch (NpgsqlException exception)
            {
                _hostLog.Fatal(StartupDatabaseFailedMessage, exception);
                hasStarted = false;
            }
            catch (InvalidOperationException exception)
            {
                _hostLog.Fatal(StoredConfigurationFailedMessage, exception);
                hasStarted = false;
            }

            return hasStarted;
        }

        private async Task StartStoredServicesAsync()
        {
            RoomStartup roomStartup = _serviceProvider.GetRequiredService<RoomStartup>();
            MatchService matchService = _serviceProvider.GetRequiredService<MatchService>();
            await roomStartup.CloseLeftoverRoomsAsync();
            TimeSpan roomInactivityLimit = await roomStartup.ReadInactivityLimitAsync();
            int codeLifetimeMinutes = await roomStartup.ReadCodeLifetimeMinutesAsync();
            ChatCapacity chatCapacity = await roomStartup.ReadChatCapacityAsync();
            SanctionPolicy sanctionPolicy = await roomStartup.ReadSanctionPolicyAsync();
            await matchService.LoadRulesAsync();
            await matchService.ResumeInterruptedMatchesAsync();

            _serviceProvider.GetRequiredService<VerificationCodeLifetime>().Start(codeLifetimeMinutes);
            _serviceProvider.GetRequiredService<ChatService>().Start(chatCapacity);
            _serviceProvider.GetRequiredService<ReportRecorder>().Start(sanctionPolicy);
            _roomInactivityLimit = roomInactivityLimit;
        }
    }
}
