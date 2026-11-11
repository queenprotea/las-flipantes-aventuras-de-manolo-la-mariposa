using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Game.Match;
using Game.Persistence;

namespace Game.Services
{
    public sealed class MatchRuleReader
    {
        private const string MissingCostMessage = "The action cost {0} is missing.";
        private const string MissingRulesMessage = "The rule catalogues do not describe {0} rounds for {1} players.";
        private const string TurnCountMismatchMessage = "Round {0} for {1} players has {2} turns but {3} build allowances.";

        private readonly TurnLayoutRepository _turnLayoutRepository;
        private readonly BuildAllowanceRepository _buildAllowanceRepository;
        private readonly ActionCostRepository _actionCostRepository;

        public MatchRuleReader(TurnLayoutRepository turnLayoutRepository, BuildAllowanceRepository buildAllowanceRepository, ActionCostRepository actionCostRepository)
        {
            ArgumentNullException.ThrowIfNull(turnLayoutRepository);
            ArgumentNullException.ThrowIfNull(buildAllowanceRepository);
            ArgumentNullException.ThrowIfNull(actionCostRepository);

            _turnLayoutRepository = turnLayoutRepository;
            _buildAllowanceRepository = buildAllowanceRepository;
            _actionCostRepository = actionCostRepository;
        }

        public async Task<IReadOnlyDictionary<int, MatchRuleSet>> ReadByPlayerCountAsync()
        {
            List<TurnLayout> turnLayouts = await _turnLayoutRepository.GetAllAsync();
            List<BuildAllowance> buildAllowances = await _buildAllowanceRepository.GetAllAsync();
            List<ActionCost> actionCosts = await _actionCostRepository.GetAllAsync();
            ActionPointCosts actionPointCosts = ToActionPointCosts(actionCosts);
            var ruleSetsByPlayerCount = new Dictionary<int, MatchRuleSet>();
            for (var playerCount = MatchCoordinator.MinimumEntrantCount; playerCount <= MatchCoordinator.MaximumEntrantCount; playerCount++)
            {
                List<RoundRule> roundRules = CreateRoundRules(playerCount, turnLayouts, buildAllowances);
                ruleSetsByPlayerCount.Add(playerCount, new MatchRuleSet(roundRules, actionPointCosts));
            }

            return ruleSetsByPlayerCount;
        }

        private static ActionPointCosts ToActionPointCosts(List<ActionCost> actionCosts)
        {
            var actionPointCosts = new ActionPointCosts
            {
                CaterpillarPlacement = FindCost(actionCosts, ActionCostCodes.CaterpillarPlacement),
                Movement = FindCost(actionCosts, ActionCostCodes.CaterpillarMovement),
                LevelClimb = FindCost(actionCosts, ActionCostCodes.LevelClimb),
                FlowerLevelAddition = FindCost(actionCosts, ActionCostCodes.FlowerLevelAddition),
            };

            return actionPointCosts;
        }

        private static int FindCost(List<ActionCost> actionCosts, string actionCode)
        {
            ActionCost? actionCost = actionCosts.FirstOrDefault(storedCost => storedCost.ActionCode == actionCode);
            if (actionCost is null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, MissingCostMessage, actionCode));
            }

            int actionPointCost = actionCost.ActionPointCost;

            return actionPointCost;
        }

        private static List<RoundRule> CreateRoundRules(int playerCount, List<TurnLayout> turnLayouts, List<BuildAllowance> buildAllowances)
        {
            List<TurnLayout> playerTurnLayouts = turnLayouts.Where(turnLayout => turnLayout.PlayerCount == playerCount).ToList();
            if (playerTurnLayouts.Count != MatchCoordinator.RoundCount)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, MissingRulesMessage, MatchCoordinator.RoundCount, playerCount));
            }

            List<RoundRule> roundRules = playerTurnLayouts.Select(turnLayout => CreateRoundRule(turnLayout, buildAllowances)).ToList();

            return roundRules;
        }

        private static RoundRule CreateRoundRule(TurnLayout turnLayout, List<BuildAllowance> buildAllowances)
        {
            List<int> buildCountsByTurn = buildAllowances.Where(buildAllowance => (buildAllowance.PlayerCount == turnLayout.PlayerCount) && (buildAllowance.RoundNumber == turnLayout.RoundNumber)).Select(ToBuildCount).ToList();
            if (buildCountsByTurn.Count != turnLayout.TurnCount)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, TurnCountMismatchMessage, turnLayout.RoundNumber, turnLayout.PlayerCount, turnLayout.TurnCount, buildCountsByTurn.Count));
            }

            var roundRule = new RoundRule(buildCountsByTurn);

            return roundRule;
        }

        private static int ToBuildCount(BuildAllowance buildAllowance)
        {
            int buildCount = buildAllowance.BuildingCount;

            return buildCount;
        }
    }
}
