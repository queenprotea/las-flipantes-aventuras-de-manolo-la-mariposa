using System;

using Game.Contracts;

using Torres.Client.Session;

namespace Torres.Client.Flows
{
    internal sealed class MatchSummaryFlow
    {
        private readonly PlayerSession _playerSession;

        internal MatchSummaryFlow(PlayerSession playerSession)
        {
            _playerSession = playerSession;
        }

        internal MatchView? CurrentMatchView => _playerSession.CurrentMatchView;

        internal DateTime? StepEndsAt => _playerSession.StepEndsAtUtc;

        internal ScreenId LeaveMatch()
        {
            _playerSession.LeaveMatch();
            ScreenId nextScreen = _playerSession.CurrentRoom is null ? ScreenId.MainMenu : ScreenId.Room;

            return nextScreen;
        }
    }
}
