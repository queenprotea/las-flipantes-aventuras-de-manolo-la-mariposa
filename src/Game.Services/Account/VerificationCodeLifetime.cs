using System;

namespace Game.Services
{
    public sealed class VerificationCodeLifetime
    {
        private const string NotStartedMessage = "The verification code lifetime was not set; Start must run before verification codes are issued.";

        private int? _lifetimeMinutes;

        public void Start(int lifetimeMinutes)
        {
            _lifetimeMinutes = lifetimeMinutes;
        }

        internal int RequireMinutes()
        {
            int lifetimeMinutes = _lifetimeMinutes ?? throw new InvalidOperationException(NotStartedMessage);

            return lifetimeMinutes;
        }
    }
}
