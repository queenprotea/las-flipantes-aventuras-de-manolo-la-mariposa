using System.Collections.Generic;
using System.Linq;

using Game.Contracts;
using Game.Persistence;

namespace Game.Services.Tests
{
    internal static class SampleMatchRules
    {
        internal const string TurnSecondsCode = "turn_seconds";
        internal const string ReconnectionSecondsCode = "reconnection_seconds";
        internal const string AbsentStepsCode = "absent_after_expired_steps";
        internal const string ResumeMinutesCode = "match_resume_minutes";
        internal const string TurnSeconds = "90";
        internal const string ReconnectionSeconds = "40";
        internal const string AbsentSteps = "3";
        internal const string ResumeMinutes = "3";
        internal const int ChanceSeed = 7;

        private const short TwoPlayers = 2;
        private const short ThreePlayers = 3;
        private const short FourPlayers = 4;
        private const short RoundOne = 1;
        private const short RoundTwo = 2;
        private const short RoundThree = 3;
        private const short OneTurnPerRound = 1;
        private const short FirstTurn = 1;
        private const short ThreeBuildings = 3;
        private const short PlacementCost = 2;
        private const short OnePoint = 1;
        private const short NoPoints = 0;

        private static readonly short[] _playerCounts = new short[] { TwoPlayers, ThreePlayers, FourPlayers };
        private static readonly short[] _roundNumbers = new short[] { RoundOne, RoundTwo, RoundThree };

        internal static IReadOnlyList<MatchCell> FiveFlowerCells { get; } = new MatchCell[]
        {
            new MatchCell { Row = 1, Column = 1 },
            new MatchCell { Row = 1, Column = 6 },
            new MatchCell { Row = 3, Column = 3 },
            new MatchCell { Row = 6, Column = 1 },
            new MatchCell { Row = 6, Column = 6 },
        };

        internal static IReadOnlyList<TurnLayout> TurnLayouts { get; } = _playerCounts.SelectMany(ListTurnLayoutsOf).ToList();

        internal static IReadOnlyList<BuildAllowance> BuildAllowances { get; } = _playerCounts.SelectMany(ListBuildAllowancesOf).ToList();

        internal static IReadOnlyList<ActionCost> ActionCosts { get; } = new ActionCost[]
        {
            new ActionCost { ActionCode = ActionCostCodes.CaterpillarPlacement, ActionPointCost = PlacementCost },
            new ActionCost { ActionCode = ActionCostCodes.CaterpillarMovement, ActionPointCost = OnePoint },
            new ActionCost { ActionCode = ActionCostCodes.LevelClimb, ActionPointCost = OnePoint },
            new ActionCost { ActionCode = ActionCostCodes.FlowerLevelAddition, ActionPointCost = OnePoint },
            new ActionCost { ActionCode = ActionCostCodes.CardDraw, ActionPointCost = OnePoint },
            new ActionCost { ActionCode = ActionCostCodes.CardPlay, ActionPointCost = NoPoints },
        };

        private static IEnumerable<TurnLayout> ListTurnLayoutsOf(short playerCount)
        {
            IEnumerable<TurnLayout> turnLayouts = _roundNumbers.Select(roundNumber => new TurnLayout { PlayerCount = playerCount, RoundNumber = roundNumber, TurnCount = OneTurnPerRound });

            return turnLayouts;
        }

        private static IEnumerable<BuildAllowance> ListBuildAllowancesOf(short playerCount)
        {
            IEnumerable<BuildAllowance> buildAllowances = _roundNumbers.Select(roundNumber => new BuildAllowance { PlayerCount = playerCount, RoundNumber = roundNumber, TurnNumber = FirstTurn, BuildingCount = ThreeBuildings });

            return buildAllowances;
        }
    }
}
