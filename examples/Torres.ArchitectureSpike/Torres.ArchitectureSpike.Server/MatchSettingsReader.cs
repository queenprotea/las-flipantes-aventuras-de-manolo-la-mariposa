using System;

using Microsoft.Extensions.Configuration;

using Torres.ArchitectureSpike.Match;

namespace Torres.ArchitectureSpike.Server
{
    public static class MatchSettingsReader
    {
        private const string SectionName = "Match";
        private const string InvalidSettingsMessage = "The Match section of appsettings.json needs positive TurnSeconds, GoalPosition and MaximumStepsPerTurn.";

        public static MatchSettings Read(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            MatchSettings? settings = configuration.GetSection(SectionName).Get<MatchSettings>();
            if ((settings is null) || (settings.TurnSeconds <= 0) || (settings.GoalPosition <= 0) || (settings.MaximumStepsPerTurn <= 0))
            {
                throw new InvalidOperationException(InvalidSettingsMessage);
            }

            return settings;
        }
    }
}
