namespace Torres.Client.Flows
{
    internal sealed record LoginOutcome(string? ErrorKey, ScreenId? NextScreen, bool IsBanned);
}
