using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Game.Services
{
    internal sealed class EmailSendingPacer
    {
        private const int RequestIntervalSeconds = 60;

        private static readonly TimeSpan _requestInterval = TimeSpan.FromSeconds(RequestIntervalSeconds);

        private readonly TimeProvider _timeProvider;
        private readonly Dictionary<string, DateTimeOffset> _lastRequestTimesByEmail = new Dictionary<string, DateTimeOffset>(StringComparer.OrdinalIgnoreCase);
        private readonly Lock _requestsLock = new Lock();

        internal EmailSendingPacer(TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(timeProvider);

            _timeProvider = timeProvider;
        }

        internal bool Admit(string emailAddress)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);

            DateTimeOffset requestTime = _timeProvider.GetUtcNow();
            bool isAdmitted;
            lock (_requestsLock)
            {
                ForgetOldRequests(requestTime);
                isAdmitted = !_lastRequestTimesByEmail.ContainsKey(emailAddress);
                if (isAdmitted)
                {
                    _lastRequestTimesByEmail[emailAddress] = requestTime;
                }
            }

            return isAdmitted;
        }

        internal void Release(string emailAddress)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);

            lock (_requestsLock)
            {
                _lastRequestTimesByEmail.Remove(emailAddress);
            }
        }

        private void ForgetOldRequests(DateTimeOffset requestTime)
        {
            List<string> oldEmails = _lastRequestTimesByEmail
                .Where(lastRequestTime => requestTime - lastRequestTime.Value >= _requestInterval)
                .Select(lastRequestTime => lastRequestTime.Key)
                .ToList();
            foreach (string oldEmail in oldEmails)
            {
                _lastRequestTimesByEmail.Remove(oldEmail);
            }
        }
    }
}
