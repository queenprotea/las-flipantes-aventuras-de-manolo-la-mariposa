using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

namespace Game.Server
{
    public sealed class MatchBoardSettings
    {
        private const string SectionName = "MatchBoard";

        public List<StartingFlowerSetting> StartingFlowers { get; set; } = new List<StartingFlowerSetting>();

        public static MatchBoardSettings Read(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            MatchBoardSettings settings = configuration.GetSection(SectionName).Get<MatchBoardSettings>() ?? new MatchBoardSettings();

            return settings;
        }
    }
}
