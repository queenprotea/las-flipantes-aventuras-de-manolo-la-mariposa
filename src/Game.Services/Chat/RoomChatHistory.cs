using System.Collections.Generic;

using Game.Contracts;

namespace Game.Services
{
    internal sealed record RoomChatHistory(IReadOnlyList<ChatMessage> Messages, IReadOnlyList<int> ClosedReportMessageIds);
}
