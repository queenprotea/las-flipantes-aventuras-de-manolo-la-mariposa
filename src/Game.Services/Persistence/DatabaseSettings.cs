using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Game.Persistence
{
    public sealed class DatabaseSettings
    {
        private const string ConnectionVariable = "TORRES_DB_CONNECTION";
        private const string MissingConnectionMessage = "The environment variable TORRES_DB_CONNECTION is not set.";

        private readonly string _connectionString;

        private DatabaseSettings(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static DatabaseSettings ReadFromEnvironment()
        {
            string? connectionString = Environment.GetEnvironmentVariable(ConnectionVariable);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(MissingConnectionMessage);
            }

            var settings = new DatabaseSettings(connectionString);

            return settings;
        }

        public void ConfigureContext(DbContextOptionsBuilder optionsBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder);

            optionsBuilder.UseNpgsql(_connectionString, ConfigureNpgsql);
        }

        private static void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder npgsqlOptions)
        {
            npgsqlOptions.ExecutionStrategy(CreateExecutionStrategy);
        }

        private static IExecutionStrategy CreateExecutionStrategy(ExecutionStrategyDependencies dependencies)
        {
            var executionStrategy = new NonRetryingExecutionStrategy(dependencies);

            return executionStrategy;
        }
    }
}
