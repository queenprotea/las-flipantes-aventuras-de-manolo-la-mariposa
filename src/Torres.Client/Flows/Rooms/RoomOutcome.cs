using Game.Contracts;

namespace Torres.Client.Flows
{
    internal sealed record RoomOutcome(Notice? Notice, ScreenId? NextScreen, StartMatchResult? BannedPlayer);
}
