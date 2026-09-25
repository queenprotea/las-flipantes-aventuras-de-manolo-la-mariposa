using System;
using System.Threading.Tasks;

using CoreWCF;

using Game.Contracts;
using Game.Persistence;

using log4net;

using Npgsql;

namespace Game.Services
{
    /// <summary>
    /// Answers the client's availability check by reading the database.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public sealed class ServerStatusService : IServerStatusService
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ServerStatusService));

        private readonly PlayerRepository _playerRepository;

        /// <summary>
        /// Creates the service over the repository used to reach the database.
        /// </summary>
        public ServerStatusService(PlayerRepository playerRepository)
        {
            ArgumentNullException.ThrowIfNull(playerRepository);

            _playerRepository = playerRepository;
        }

        /// <inheritdoc/>
        public async Task<ServerStatus> GetStatusAsync()
        {
            long playerCount;
            try
            {
                playerCount = await _playerRepository.CountAsync();
            }
            catch (NpgsqlException exception)
            {
                _log.Error("The database could not be read during the status check.", exception);
                return ServerStatus.DatabaseUnavailable;
            }

            _log.InfoFormat("Status check answered; the player table has {0} rows.", playerCount);
            return ServerStatus.Ready;
        }
    }
}
