using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Game.Persistence
{
    public sealed class SystemParameterRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public SystemParameterRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);

            _contextFactory = contextFactory;
        }

        public async Task<string?> FindValueAsync(string parameterCode)
        {
            ArgumentNullException.ThrowIfNull(parameterCode);

            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            SystemParameter? parameter = await databaseContext.SystemParameters
                .AsNoTracking()
                .SingleOrDefaultAsync(storedParameter => storedParameter.ParameterCode == parameterCode);
            string? parameterValue = parameter?.ParameterValue;

            return parameterValue;
        }
    }
}
