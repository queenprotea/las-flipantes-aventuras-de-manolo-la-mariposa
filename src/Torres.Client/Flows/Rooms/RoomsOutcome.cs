using System.Collections.Generic;

using Game.Contracts;

namespace Torres.Client.Flows
{
    internal sealed record RoomsOutcome(List<RoomSummary>? Rooms, Notice? Notice, ScreenId? NextScreen);
}
