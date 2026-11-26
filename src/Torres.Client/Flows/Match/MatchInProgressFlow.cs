using Torres.Client.Connection;
using Torres.Client.Session;

namespace Torres.Client.Flows
{
    internal sealed class MatchInProgressFlow
    {
        private readonly PlayerSession _playerSession;
        private readonly ForfeitFlow _forfeitFlow;

        internal MatchInProgressFlow(GameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _playerSession = playerSession;
            _forfeitFlow = new ForfeitFlow(gameSessionConnection, playerSession);
        }

        internal string MatchRoomCode => _playerSession.MatchRoomCode ?? string.Empty;

        internal bool IsAbandoning => _forfeitFlow.IsForfeiting;

        internal void Open()
        {
            _forfeitFlow.Forget();
        }

        internal void Abandon()
        {
            _forfeitFlow.Forfeit();
        }

        internal ScreenOutcome? TakeOutcome()
        {
            ScreenOutcome? forfeitOutcome = _forfeitFlow.TakeOutcome();
            ScreenOutcome? outcome = forfeitOutcome;
            if (forfeitOutcome?.NextScreen is not null)
            {
                outcome = new ScreenOutcome(null, ScreenId.MainMenu);
            }

            return outcome;
        }
    }
}
