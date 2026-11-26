namespace Torres.Client.Flows
{
    internal sealed record InvitePlayersOutcome(bool NeedsRender, string? ErrorKey, ScreenId? NextScreen);
}
