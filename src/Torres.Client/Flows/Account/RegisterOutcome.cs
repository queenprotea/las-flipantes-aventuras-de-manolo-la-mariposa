namespace Torres.Client.Flows
{
    internal sealed record RegisterOutcome(RegisterStep? Step, RegisterField? MessageField, string? MessageKey, ScreenId? NextScreen);
}
