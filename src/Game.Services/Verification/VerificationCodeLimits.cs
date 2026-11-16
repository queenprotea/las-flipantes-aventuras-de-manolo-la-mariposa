namespace Game.Services
{
    public static class VerificationCodeLimits
    {
        public const int MaximumFailedAttempts = 5;

        public const string LifetimeParameterCode = "recovery_code_minutes";
    }
}
