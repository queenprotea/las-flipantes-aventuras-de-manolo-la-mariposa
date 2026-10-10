using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Torres.EfCoreSpike.Persistence
{
    public static class DatabaseSettings
    {
        private const string ConnectionVariable = "TORRES_DB_CONNECTION";
        private const string MissingConnectionMessage = "The environment variable TORRES_DB_CONNECTION is not set.";

        public static DbContextOptions<TorresDbContext> CreateOptions(Action<string>? sqlLogger)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TorresDbContext>();
            optionsBuilder.UseNpgsql(ReadConnectionString());
            if (sqlLogger is not null)
            {
                optionsBuilder.LogTo(sqlLogger, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
            }

            DbContextOptions<TorresDbContext> contextOptions = optionsBuilder.Options;

            return contextOptions;
        }

        private static string ReadConnectionString()
        {
            string? connectionString = Environment.GetEnvironmentVariable(ConnectionVariable);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(MissingConnectionMessage);
            }

            return connectionString;
        }
    }
}
