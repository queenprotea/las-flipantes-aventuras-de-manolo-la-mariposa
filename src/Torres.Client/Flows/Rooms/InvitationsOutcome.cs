namespace Torres.Client.Flows
{
    internal sealed record InvitationsOutcome(Notice? Notice, ScreenId? NextScreen, bool AsksRoomFullChoice);
}
