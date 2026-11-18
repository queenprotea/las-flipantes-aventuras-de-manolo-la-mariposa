using System;
using System.Collections.Generic;
using System.Linq;

using Game.Domain;
using Game.Persistence;

namespace Game.Match
{
    internal static class MatchRecordConversion
    {
        private const int FirstPosition = 1;

        internal static List<MatchParticipant> ToSeatedPlayers(IReadOnlyList<SeatedEntrant> seatedEntrants)
        {
            List<MatchParticipant> seatedPlayers = seatedEntrants.Where(IsPlayer).Select(ToSeatedPlayer).ToList();

            return seatedPlayers;
        }

        internal static List<MatchParticipant> ToPlayerResults(MatchTable matchTable, IReadOnlyList<SeatedEntrant> seatedEntrants)
        {
            List<int> finishingSeatNumbers = matchTable.FindFinishingOrder().Select(standing => standing.SeatNumber).ToList();
            List<MatchParticipant> playerResults = seatedEntrants
                .Where(IsPlayer)
                .Select(seatedEntrant => ToPlayerResult(seatedEntrant, matchTable.Standings[seatedEntrant.SeatNumber - MatchTable.FirstSeatNumber], finishingSeatNumbers))
                .ToList();

            return playerResults;
        }

        internal static string ToEndReason(FinishCause? finishCause)
        {
            string endReason = finishCause switch
            {
                FinishCause.Completed => MatchEndReasonCodes.Completed,
                FinishCause.TooFewSeats => MatchEndReasonCodes.Abandoned,
                null => MatchEndReasonCodes.Abandoned,
                _ => throw new ArgumentOutOfRangeException(nameof(finishCause)),
            };

            return endReason;
        }

        private static bool IsPlayer(SeatedEntrant seatedEntrant)
        {
            bool isPlayer = !seatedEntrant.Entrant.IsGuest;

            return isPlayer;
        }

        private static MatchParticipant ToSeatedPlayer(SeatedEntrant seatedEntrant)
        {
            var seatedPlayer = new MatchParticipant
            {
                SeatNumber = (short)seatedEntrant.SeatNumber,
                PlayerId = seatedEntrant.Entrant.EntrantId,
            };

            return seatedPlayer;
        }

        private static MatchParticipant ToPlayerResult(SeatedEntrant seatedEntrant, SeatStanding standing, List<int> finishingSeatNumbers)
        {
            MatchParticipant playerResult = ToSeatedPlayer(seatedEntrant);
            playerResult.WasWithdrawn = standing.IsRetired;
            playerResult.FinalScore = standing.FinalScore;
            playerResult.FinalPosition = (short)(finishingSeatNumbers.IndexOf(seatedEntrant.SeatNumber) + FirstPosition);

            return playerResult;
        }
    }
}
