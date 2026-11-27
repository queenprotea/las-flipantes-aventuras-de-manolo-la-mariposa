using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

using Microsoft.Extensions.Configuration;

namespace Game.Server
{
    public sealed class MatchBoardSettings
    {
        private const string SectionName = "MatchBoard";

        public List<StartingFlowerSetting> StartingFlowers { get; set; } = new List<StartingFlowerSetting>();

        public static MatchBoardSettings ReadFromConfiguration(IConfiguration configuration)
        {
            MatchBoardSettings settings = configuration.GetSection(SectionName).Get<MatchBoardSettings>() ?? new MatchBoardSettings();

            return settings;
        }

        public List<MatchCell> ToStartingCells()
        {
            List<MatchCell> startingCells = StartingFlowers.Select(ToMatchCell).ToList();

            return startingCells;
        }

        private static MatchCell ToMatchCell(StartingFlowerSetting startingFlower)
        {
            var matchCell = new MatchCell
            {
                Row = startingFlower.Row,
                Column = startingFlower.Column,
            };

            return matchCell;
        }
    }
}
