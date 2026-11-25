using System;

using Microsoft.Extensions.Configuration;

namespace Game.Server
{
    public sealed class HistorySettings
    {
        private const string SectionName = "History";
        private const string InvalidPageSizeMessage = "The History section of appsettings.json needs a positive MatchesPerPage.";

        public int MatchesPerPage { get; set; }

        public static HistorySettings Read(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            HistorySettings? settings = configuration.GetSection(SectionName).Get<HistorySettings>();
            if ((settings is null) || (settings.MatchesPerPage <= 0))
            {
                throw new InvalidOperationException(InvalidPageSizeMessage);
            }

            return settings;
        }
    }
}
