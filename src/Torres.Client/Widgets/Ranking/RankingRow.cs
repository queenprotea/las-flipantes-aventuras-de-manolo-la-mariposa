using System.Collections.Generic;

using Game.Contracts;

namespace Torres.Client.Widgets
{
    internal sealed record RankingRow(int Rank, string DisplayName, bool IsViewer, SeatColor? Color, IReadOnlyList<int> StatValues);
}
