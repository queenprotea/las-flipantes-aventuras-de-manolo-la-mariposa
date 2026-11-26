namespace Torres.Client.Flows
{
    internal sealed record ResumeOutcome(Notice? Notice, bool IsMatchLost);
}
