namespace Torres.Client.Flows
{
    internal sealed record AccountDeletionOutcome(string? ErrorKey, bool WasDeleted);
}
