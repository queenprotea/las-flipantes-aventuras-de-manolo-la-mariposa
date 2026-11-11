using System;
using System.Globalization;

using CoreWCF;

using log4net;

namespace Game.Services
{
    internal static class AccountOwnerGuard
    {
        private const string SessionNotOpenMessage = "The session has not been opened with a valid session token.";
        private const string GuestNotAllowedMessage = "Guests cannot use {0}.";
        private const string GuestRejectedMessage = "Guest {0} tried to use {1} and was rejected.";

        internal static SessionOwner RequireAccountOwner(SessionRegistry sessionRegistry, ILog featureLog, string featureName)
        {
            SessionOwner accountOwner = RequireAccountSession(sessionRegistry, featureLog, featureName).Owner;

            return accountOwner;
        }

        internal static PlayerSession RequireAccountSession(SessionRegistry sessionRegistry, ILog featureLog, string featureName)
        {
            ArgumentNullException.ThrowIfNull(sessionRegistry);
            ArgumentNullException.ThrowIfNull(featureLog);
            ArgumentException.ThrowIfNullOrWhiteSpace(featureName);

            PlayerSession? caller = sessionRegistry.FindCaller();
            if (caller is null)
            {
                throw new FaultException(SessionNotOpenMessage);
            }

            if (caller.Owner.IsGuest)
            {
                featureLog.WarnFormat(CultureInfo.InvariantCulture, GuestRejectedMessage, caller.Owner.OwnerId, featureName);
                string rejectionReason = string.Format(CultureInfo.InvariantCulture, GuestNotAllowedMessage, featureName);
                throw new FaultException(rejectionReason);
            }

            return caller;
        }
    }
}
