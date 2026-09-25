using System;

using Npgsql;

namespace Game.Persistence
{
    public static class DatabaseSettings
    {
        private const string HostVariable = "TORRES_DB_HOST";
        private const string UserVariable = "TORRES_DB_USER";
        private const string PasswordVariable = "TORRES_DB_PASSWORD";
        private const string DefaultHost = "localhost";
        private const int DatabasePort = 5432;
        private const string DatabaseName = "torres";
        private const string SchemaName = "torres";
        private const int ConnectionTimeoutSeconds = 5;

        public static NpgsqlDataSource CreateDataSource()
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = Environment.GetEnvironmentVariable(HostVariable) ?? DefaultHost,
                Port = DatabasePort,
                Database = DatabaseName,
                Username = ReadRequired(UserVariable),
                Password = ReadRequired(PasswordVariable),
                SearchPath = SchemaName,
                Timeout = ConnectionTimeoutSeconds,
            };

            return NpgsqlDataSource.Create(builder.ConnectionString);
        }

        private static string ReadRequired(string variableName)
        {
            string? value = Environment.GetEnvironmentVariable(variableName);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"The environment variable {variableName} is not set.");
            }

            return value;
        }
    }
}
