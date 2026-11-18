using System.Collections.Generic;
using System.Linq;

using Game.Contracts;
using Game.Match;

namespace Game.Services
{
    internal sealed class RoomRanking
    {
        private const int WinAdded = 1;
        private const int NoWinAdded = 0;

        private readonly Dictionary<int, RoomRankingTally> _talliesByMemberId = new Dictionary<int, RoomRankingTally>();

        internal void Add(EntrantStanding entrantStanding)
        {
            RoomRankingTally previousTally = _talliesByMemberId.GetValueOrDefault(entrantStanding.MemberId);
            int addedWins = entrantStanding.IsWinner ? WinAdded : NoWinAdded;
            _talliesByMemberId[entrantStanding.MemberId] = new RoomRankingTally(previousTally.Points + entrantStanding.Points, previousTally.Wins + addedWins);
        }

        internal void Remove(int memberId)
        {
            _talliesByMemberId.Remove(memberId);
        }

        internal List<RoomRankingEntry> ListEntries(IReadOnlyList<LiveRoomMember> members)
        {
            List<RoomRankingEntry> entries = members.Where(HasTally).Select(ToEntry).OrderByDescending(entry => entry.Points).ThenByDescending(entry => entry.Wins).ThenBy(entry => entry.MemberId).ToList();

            return entries;
        }

        private bool HasTally(LiveRoomMember member)
        {
            bool hasTally = _talliesByMemberId.ContainsKey(member.MemberId);

            return hasTally;
        }

        private RoomRankingEntry ToEntry(LiveRoomMember member)
        {
            RoomRankingTally tally = _talliesByMemberId[member.MemberId];
            var entry = new RoomRankingEntry
            {
                MemberId = member.MemberId,
                DisplayName = member.DisplayName,
                IsGuest = member.IsGuest,
                Color = member.Color,
                Points = tally.Points,
                Wins = tally.Wins,
            };

            return entry;
        }
    }
}
