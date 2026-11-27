using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Game.Services
{
    public sealed class PendingRegistrationRegistry
    {
        private readonly TimeProvider _timeProvider;
        private readonly Dictionary<string, PendingRegistration> _registrationsByEmail = new Dictionary<string, PendingRegistration>(StringComparer.OrdinalIgnoreCase);
        private readonly Lock _registryLock = new Lock();

        public PendingRegistrationRegistry(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public void Hold(PendingRegistration registration)
        {
            lock (_registryLock)
            {
                ForgetExpired();
                _registrationsByEmail[registration.Email] = registration;
            }
        }

        public PendingRegistration? Redeem(string email, string code)
        {
            PendingRegistration? redeemedRegistration;
            lock (_registryLock)
            {
                ForgetExpired();
                redeemedRegistration = TakeMatching(email, code);
            }

            return redeemedRegistration;
        }

        private PendingRegistration? TakeMatching(string email, string code)
        {
            if (!_registrationsByEmail.TryGetValue(email, out PendingRegistration? heldRegistration))
            {
                return null;
            }

            PendingRegistration? matchingRegistration = null;
            if (string.Equals(heldRegistration.Code, code, StringComparison.Ordinal))
            {
                matchingRegistration = heldRegistration;
                _registrationsByEmail.Remove(email);
            }
            else
            {
                ForgetIfExhausted(heldRegistration);
            }

            return matchingRegistration;
        }

        private void ForgetIfExhausted(PendingRegistration heldRegistration)
        {
            heldRegistration.RecordFailedAttempt();
            if (heldRegistration.FailedAttempts >= VerificationCodeLimits.MaximumFailedAttempts)
            {
                _registrationsByEmail.Remove(heldRegistration.Email);
            }
        }

        private void ForgetExpired()
        {
            DateTimeOffset now = _timeProvider.GetUtcNow();
            List<string> expiredEmails = _registrationsByEmail
                .Where(registration => registration.Value.ExpiresAt <= now)
                .Select(registration => registration.Key)
                .ToList();
            foreach (string expiredEmail in expiredEmails)
            {
                _registrationsByEmail.Remove(expiredEmail);
            }
        }
    }
}
