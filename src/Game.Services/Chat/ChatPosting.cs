using System.Collections.Generic;

using Game.Contracts;

namespace Game.Services
{
    internal sealed record ChatPosting(ChatMessage Message, IReadOnlyList<int> RecipientOwnerIds);
}
