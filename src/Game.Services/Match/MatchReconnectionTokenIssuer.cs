using System;

using Game.Match;

namespace Game.Services
{
    internal sealed class MatchReconnectionTokenIssuer
    {
        private readonly SessionTokenIssuer _sessionTokenIssuer;
        private readonly SessionRegistry _sessionRegistry;

        private TimeSpan _tokenLifetime;

        internal MatchReconnectionTokenIssuer(SessionTokenIssuer sessionTokenIssuer, SessionRegistry sessionRegistry)
        {
            _sessionTokenIssuer = sessionTokenIssuer;
            _sessionRegistry = sessionRegistry;
        }

        internal void UseLifetime(TimeSpan tokenLifetime)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(tokenLifetime, TimeSpan.Zero);

            _tokenLifetime = tokenLifetime;
        }

        internal void Attach(MatchRecipientView recipientView)
        {
            SessionOwner? viewerOwner = _sessionRegistry.FindOwner(recipientView.EntrantId);
            if ((viewerOwner is not null) && (_tokenLifetime > TimeSpan.Zero))
            {
                recipientView.MatchView.ReconnectionToken = _sessionTokenIssuer.IssueForMatch(recipientView.MatchId, viewerOwner, _tokenLifetime);
            }
        }
    }
}
