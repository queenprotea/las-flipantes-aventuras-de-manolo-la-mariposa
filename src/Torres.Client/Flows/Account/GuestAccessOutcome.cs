namespace Torres.Client.Flows
{
    internal sealed record GuestAccessOutcome(string? ErrorKey, ScreenId? NextScreen, bool IsServerUnreachable);
}
