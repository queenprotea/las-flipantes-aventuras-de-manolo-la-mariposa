namespace Torres.Client.Flows
{
    internal sealed record DisconnectionOutcome(Notice? Notice, bool IsResumed, bool IsSessionRefused, bool HasMatchEndedWhileAway, bool IsMatchAwaitingPlayers);
}
