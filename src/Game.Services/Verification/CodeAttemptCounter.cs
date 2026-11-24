using System.Collections.Generic;
using System.Threading;

namespace Game.Services
{
    internal sealed class CodeAttemptCounter
    {
        private readonly Dictionary<int, int> _failedAttemptsByPlayerId = new Dictionary<int, int>();
        private readonly Lock _attemptsLock = new Lock();

        internal bool HasAttemptsLeft(int playerId)
        {
            bool hasAttemptsLeft;
            lock (_attemptsLock)
            {
                int failedAttempts = _failedAttemptsByPlayerId.GetValueOrDefault(playerId);
                hasAttemptsLeft = failedAttempts < VerificationCodeLimits.MaximumFailedAttempts;
            }

            return hasAttemptsLeft;
        }

        internal void RecordFailure(int playerId)
        {
            lock (_attemptsLock)
            {
                _failedAttemptsByPlayerId[playerId] = _failedAttemptsByPlayerId.GetValueOrDefault(playerId) + 1;
            }
        }

        internal void Forget(int playerId)
        {
            lock (_attemptsLock)
            {
                _failedAttemptsByPlayerId.Remove(playerId);
            }
        }
    }
}
