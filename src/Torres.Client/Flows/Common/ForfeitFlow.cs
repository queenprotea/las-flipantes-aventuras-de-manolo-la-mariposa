using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class ForfeitFlow
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<ForfeitResult> _forfeitRequest = new PendingRequest<ForfeitResult>();

        internal ForfeitFlow(GameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal bool IsForfeiting => _forfeitRequest.IsWaiting;

        internal void Forfeit()
        {
            if (!_forfeitRequest.IsWaiting)
            {
                _forfeitRequest.Start(_gameSessionConnection.ForfeitAsync());
            }
        }

        internal void Forget()
        {
            _forfeitRequest.Forget();
        }

        internal ScreenOutcome? TakeOutcome()
        {
            FinishedRequest<ForfeitResult>? finishedForfeit = _forfeitRequest.TakeFinished();
            ScreenOutcome? outcome = null;
            if ((finishedForfeit is not null) && finishedForfeit.IsSuccessful && (finishedForfeit.Reply != ForfeitResult.NotIdentified))
            {
                _playerSession.ForfeitMatch();
                ScreenId nextScreen = _playerSession.CurrentRoom is null ? ScreenId.MainMenu : ScreenId.Room;
                outcome = new ScreenOutcome(null, nextScreen);
            }
            else if (finishedForfeit is not null)
            {
                outcome = new ScreenOutcome(Notice.CreateServerError(), null);
            }

            return outcome;
        }
    }
}
