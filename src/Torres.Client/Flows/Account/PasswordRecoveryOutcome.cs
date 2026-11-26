namespace Torres.Client.Flows
{
    internal sealed record PasswordRecoveryOutcome(PasswordRecoveryStep? Step, PasswordRecoveryStep? MessageStep, string? MessageKey, PasswordRecoveryEnding? Ending);
}
