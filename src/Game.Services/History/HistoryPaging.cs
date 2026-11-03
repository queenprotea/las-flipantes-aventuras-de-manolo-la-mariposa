using System;

namespace Game.Services
{
    public sealed class HistoryPaging
    {
        public HistoryPaging(int matchesPerPage)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(matchesPerPage);

            MatchesPerPage = matchesPerPage;
        }

        public int MatchesPerPage { get; }
    }
}
