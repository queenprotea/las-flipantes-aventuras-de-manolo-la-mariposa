using System.Collections.Generic;

using Game.Contracts;

namespace Torres.Client.Screens
{
    internal sealed record RankingRow(int Rank, string DisplayName, bool IsViewer, SeatColor? Color, IReadOnlyList<int> StatValues);
}
