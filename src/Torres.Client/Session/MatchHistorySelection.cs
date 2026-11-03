using System;

namespace Torres.Client.Session
{
    internal sealed class MatchHistorySelection
    {
        internal const int FirstPageNumber = 1;

        private int _pageToResume = FirstPageNumber;

        internal int? SelectedMatchId { get; private set; }

        internal void Select(int matchId, int pageNumber)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(matchId);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber);

            SelectedMatchId = matchId;
            _pageToResume = pageNumber;
        }

        internal int TakePageToResume()
        {
            int pageNumber = SelectedMatchId.HasValue ? _pageToResume : FirstPageNumber;
            Clear();

            return pageNumber;
        }

        internal void Clear()
        {
            SelectedMatchId = null;
            _pageToResume = FirstPageNumber;
        }
    }
}
