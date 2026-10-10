using System;
using System.Threading.Tasks;

using CoreWCF;

using Game.Contracts;
using Game.Persistence;

using log4net;

using Npgsql;

namespace Game.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public sealed class ServerStatusService : IServerStatusService
    {
        private const string StatusReadFailedMessage = "The database could not be read during the status check.";
        private const string StatusAnsweredMessage = "Status check answered; the player table has {0} rows.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(ServerStatusService));

        private readonly PlayerRepository _playerRepository;

        public ServerStatusService(PlayerRepository playerRepository)
        {
            ArgumentNullException.ThrowIfNull(playerRepository);

            _playerRepository = playerRepository;
        }

        public async Task<ServerStatus> GetStatusAsync()
        {
            long playerCount;
            try
            {
                playerCount = await _playerRepository.CountAsync();
            }
            catch (NpgsqlException exception)
            {
                _log.Error(StatusReadFailedMessage, exception);
                return ServerStatus.DatabaseUnavailable;
            }

            _log.InfoFormat(StatusAnsweredMessage, playerCount);

            return ServerStatus.Ready;
        }
    }
}
