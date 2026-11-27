namespace Game.Services
{
    public sealed class HistoryPaging
    {
        public HistoryPaging(int matchesPerPage)
        {
            MatchesPerPage = matchesPerPage;
        }

        public int MatchesPerPage { get; }
    }
}
