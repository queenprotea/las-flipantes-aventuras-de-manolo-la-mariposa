using Game.Contracts;

namespace Torres.Client.Scene
{
    internal readonly record struct HandCardState(MatchCard Card, bool IsPlayable, bool IsChosen);
}
