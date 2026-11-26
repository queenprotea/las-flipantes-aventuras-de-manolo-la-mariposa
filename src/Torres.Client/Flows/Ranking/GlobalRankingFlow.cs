using System;
using System.Collections.Generic;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class GlobalRankingFlow
    {
        private readonly RankingConnection _rankingConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<GlobalRankingResponse> _rankingRequest = new PendingRequest<GlobalRankingResponse>();

        private bool _isRequested;

        internal GlobalRankingFlow(RankingConnection rankingConnection, PlayerSession playerSession)
        {
            _rankingConnection = rankingConnection;
            _playerSession = playerSession;
        }

        internal GlobalRankingOutcome? TakeOutcome()
        {
            if (!_isRequested)
            {
                _isRequested = true;
                _rankingRequest.Start(_rankingConnection.GetGlobalRankingAsync());
            }

            GlobalRankingOutcome? outcome = null;
            FinishedRequest<GlobalRankingResponse>? finishedRanking = _rankingRequest.TakeFinished();
            if (finishedRanking is not null)
            {
                _rankingConnection.ReleaseChannel();
                List<RankingEntry>? entries = null;
                if (finishedRanking.IsSuccessful && (finishedRanking.Reply is GlobalRankingResponse ranking))
                {
                    entries = ranking.Entries;
                }

                outcome = new GlobalRankingOutcome(entries);
            }

            return outcome;
        }

        internal void Retry()
        {
            _rankingConnection.ReleaseChannel();
            _rankingRequest.Forget();
            _isRequested = false;
        }

        internal bool IsCurrentPlayer(string playerName)
        {
            bool isCurrentPlayer = (_playerSession.Player is PlayerIdentity player)
                && string.Equals(player.Username, playerName, StringComparison.Ordinal);

            return isCurrentPlayer;
        }
    }
}
