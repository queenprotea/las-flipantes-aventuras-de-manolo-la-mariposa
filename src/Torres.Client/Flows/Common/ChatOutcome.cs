namespace Torres.Client.Flows
{
    internal sealed record ChatOutcome(string? NoticeKey, bool IsReported, string? AcceptedText);
}
