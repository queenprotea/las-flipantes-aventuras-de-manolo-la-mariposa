using System;

namespace Game.Services
{
    internal sealed record ChatDraft(string Text, DateTime SentAtUtc);
}
