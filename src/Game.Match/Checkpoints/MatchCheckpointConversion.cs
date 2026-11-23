using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Game.Domain;

namespace Game.Match
{
    internal static class MatchCheckpointConversion
    {
        private const string MissingPartMessage = "The saved match state is missing a part.";
        private const string SeatOrderMessage = "The saved match state must number its seats from 1 in order, each with a different player or guest.";
        private const string SeatReferenceMessage = "The saved match state names a seat that does not exist.";
        private const int NoEntrantId = 0;

        internal static MatchCheckpoint ToCheckpoint(MatchSnapshot stepSnapshot, string roomCode, IReadOnlyList<CheckpointSeat> seats)
        {
            var checkpoint = new MatchCheckpoint
            {
                RoomCode = roomCode,
                Seats = seats.ToList(),
                Phase = stepSnapshot.Phase,
                RoundNumber = stepSnapshot.RoundNumber,
                TurnRoundNumber = stepSnapshot.TurnRoundNumber,
                ActiveSeatNumber = stepSnapshot.ActiveSeatNumber,
                ActionPointsLeft = stepSnapshot.ActionPointsLeft,
                BuildsLeft = stepSnapshot.BuildsLeft,
                Flowers = stepSnapshot.Flowers.Select(ToCheckpointFlower).ToList(),
                Caterpillars = stepSnapshot.Caterpillars.Select(ToCheckpointCaterpillar).ToList(),
                ButterflyCell = stepSnapshot.ButterflyCell is BoardCell butterflyCell ? ToCheckpointCell(butterflyCell) : null,
                Standings = stepSnapshot.Standings.Select(ToCheckpointStanding).ToList(),
                SeatCards = stepSnapshot.SeatCards.Select(ToCheckpointSeatCards).ToList(),
                PendingButterflySeatNumbers = stepSnapshot.PendingButterflySeatNumbers.ToList(),
            };

            return checkpoint;
        }

        internal static CheckpointSeat ToCheckpointSeat(SeatedEntrant seatedEntrant, bool isRetired)
        {
            var checkpointSeat = new CheckpointSeat
            {
                SeatNumber = seatedEntrant.SeatNumber,
                EntrantId = seatedEntrant.Entrant.EntrantId,
                MemberId = seatedEntrant.Entrant.MemberId,
                DisplayName = seatedEntrant.Entrant.DisplayName,
                IsGuest = seatedEntrant.Entrant.IsGuest,
                Color = seatedEntrant.Entrant.SeatColor,
                IsRetired = isRetired,
            };

            return checkpointSeat;
        }

        internal static MatchSnapshot ToSnapshot(MatchCheckpoint checkpoint)
        {
            int seatCount = Require(checkpoint.Seats).Count;
            List<Caterpillar> caterpillars = Require(checkpoint.Caterpillars).Select(ToCaterpillar).ToList();
            List<SeatStanding> standings = Require(checkpoint.Standings).Select(ToStanding).ToList();
            List<SeatCards> seatCards = Require(checkpoint.SeatCards).Select(ToSeatCards).ToList();
            List<int> pendingButterflySeatNumbers = Require(checkpoint.PendingButterflySeatNumbers);
            EnsureSeatReferences(caterpillars.Select(caterpillar => caterpillar.SeatNumber).Concat(pendingButterflySeatNumbers), seatCount);
            EnsureSeatOrder(standings.Select(standing => standing.SeatNumber).ToList());
            EnsureSeatOrder(seatCards.Select(cards => cards.SeatNumber).ToList());
            BoardCell? butterflyCell = checkpoint.ButterflyCell is null ? null : ToBoardCell(checkpoint.ButterflyCell);
            List<Flower> flowers = Require(checkpoint.Flowers).Select(ToFlower).ToList();
            FinishCause? noFinishCause = null;
            var snapshot = new MatchSnapshot(checkpoint.Phase, checkpoint.RoundNumber, checkpoint.TurnRoundNumber, seatCount, checkpoint.ActiveSeatNumber, checkpoint.ActionPointsLeft, checkpoint.BuildsLeft, flowers, caterpillars, butterflyCell, standings, seatCards, pendingButterflySeatNumbers, noFinishCause);

            return snapshot;
        }

        internal static List<SeatedEntrant> ToSeatedEntrants(MatchCheckpoint checkpoint)
        {
            List<CheckpointSeat> seats = Require(checkpoint.Seats).Select(Require).ToList();
            EnsureSeatOrder(seats.Select(seat => seat.SeatNumber).ToList());
            bool hasDistinctEntrants = seats.Select(seat => seat.EntrantId).Distinct().Count() == seats.Count;
            bool hasValidEntrants = seats.All(seat => (seat.EntrantId != NoEntrantId) && (seat.IsGuest == (seat.EntrantId < NoEntrantId)));
            if (!hasDistinctEntrants || !hasValidEntrants)
            {
                throw new ArgumentException(SeatOrderMessage, nameof(checkpoint));
            }

            List<SeatedEntrant> seatedEntrants = seats.Select(ToSeatedEntrant).ToList();

            return seatedEntrants;
        }

        internal static List<int> FindRetiredSeatNumbers(MatchCheckpoint checkpoint)
        {
            List<int> retiredSeatNumbers = Require(checkpoint.Seats).Select(Require).Where(seat => seat.IsRetired).Select(seat => seat.SeatNumber).ToList();

            return retiredSeatNumbers;
        }

        private static TCheckpointPart Require<TCheckpointPart>(TCheckpointPart? checkpointPart)
            where TCheckpointPart : class
        {
            TCheckpointPart requiredPart = checkpointPart ?? throw new SerializationException(MissingPartMessage);

            return requiredPart;
        }

        private static void EnsureSeatOrder(List<int> seatNumbers)
        {
            bool isNumberedInOrder = seatNumbers.Select((seatNumber, seatIndex) => seatNumber == MatchTable.FirstSeatNumber + seatIndex).All(isInPlace => isInPlace);
            if (!isNumberedInOrder)
            {
                throw new ArgumentException(SeatOrderMessage, nameof(seatNumbers));
            }
        }

        private static void EnsureSeatReferences(IEnumerable<int> seatNumbers, int seatCount)
        {
            bool isEverySeatKnown = seatNumbers.All(seatNumber => (seatNumber >= MatchTable.FirstSeatNumber) && (seatNumber < MatchTable.FirstSeatNumber + seatCount));
            if (!isEverySeatKnown)
            {
                throw new ArgumentException(SeatReferenceMessage, nameof(seatNumbers));
            }
        }

        private static SeatedEntrant ToSeatedEntrant(CheckpointSeat seat)
        {
            const bool isSeatedActive = true;
            var entrant = new MatchEntrant(seat.EntrantId, seat.MemberId, Require(seat.DisplayName), seat.IsGuest, isSeatedActive, seat.Color);
            var seatedEntrant = new SeatedEntrant(seat.SeatNumber, entrant);

            return seatedEntrant;
        }

        private static CheckpointCell ToCheckpointCell(BoardCell boardCell)
        {
            var checkpointCell = new CheckpointCell { Row = boardCell.Row, Column = boardCell.Column };

            return checkpointCell;
        }

        private static BoardCell ToBoardCell(CheckpointCell? checkpointCell)
        {
            CheckpointCell requiredCell = Require(checkpointCell);
            var boardCell = new BoardCell(requiredCell.Row, requiredCell.Column);

            return boardCell;
        }

        private static CheckpointFlower ToCheckpointFlower(Flower flower)
        {
            var checkpointFlower = new CheckpointFlower { Cell = ToCheckpointCell(flower.Cell), Level = flower.Level };

            return checkpointFlower;
        }

        private static Flower ToFlower(CheckpointFlower? checkpointFlower)
        {
            CheckpointFlower requiredFlower = Require(checkpointFlower);
            var flower = new Flower(ToBoardCell(requiredFlower.Cell), requiredFlower.Level);

            return flower;
        }

        private static CheckpointCaterpillar ToCheckpointCaterpillar(Caterpillar caterpillar)
        {
            var checkpointCaterpillar = new CheckpointCaterpillar { SeatNumber = caterpillar.SeatNumber, Cell = ToCheckpointCell(caterpillar.Cell) };

            return checkpointCaterpillar;
        }

        private static Caterpillar ToCaterpillar(CheckpointCaterpillar? checkpointCaterpillar)
        {
            CheckpointCaterpillar requiredCaterpillar = Require(checkpointCaterpillar);
            var caterpillar = new Caterpillar(requiredCaterpillar.SeatNumber, ToBoardCell(requiredCaterpillar.Cell));

            return caterpillar;
        }

        private static CheckpointStanding ToCheckpointStanding(SeatStanding standing)
        {
            var checkpointStanding = new CheckpointStanding
            {
                SeatNumber = standing.SeatNumber,
                RoundScores = standing.RoundScores.Select(ToCheckpointRoundScore).ToList(),
                IsRetired = standing.IsRetired,
                CardPoints = standing.CardPoints,
            };

            return checkpointStanding;
        }

        private static SeatStanding ToStanding(CheckpointStanding? checkpointStanding)
        {
            CheckpointStanding requiredStanding = Require(checkpointStanding);
            List<SeatRoundScore> roundScores = Require(requiredStanding.RoundScores).Select(ToRoundScore).ToList();
            var standing = new SeatStanding(requiredStanding.SeatNumber, roundScores, requiredStanding.IsRetired, requiredStanding.CardPoints);

            return standing;
        }

        private static CheckpointRoundScore ToCheckpointRoundScore(SeatRoundScore roundScore)
        {
            var checkpointRoundScore = new CheckpointRoundScore { PlanterPoints = roundScore.PlanterPoints, ButterflyPoints = roundScore.ButterflyPoints, ScoredPlanterCount = roundScore.ScoredPlanterCount };

            return checkpointRoundScore;
        }

        private static SeatRoundScore ToRoundScore(CheckpointRoundScore? checkpointRoundScore)
        {
            CheckpointRoundScore requiredRoundScore = Require(checkpointRoundScore);
            var roundScore = new SeatRoundScore(requiredRoundScore.PlanterPoints, requiredRoundScore.ButterflyPoints, requiredRoundScore.ScoredPlanterCount);

            return roundScore;
        }

        private static CheckpointSeatCards ToCheckpointSeatCards(SeatCards seatCards)
        {
            var checkpointSeatCards = new CheckpointSeatCards
            {
                SeatNumber = seatCards.SeatNumber,
                Hand = seatCards.Hand.ToList(),
                Deck = seatCards.Deck.ToList(),
                HasUsedPointsCard = seatCards.HasUsedPointsCard,
            };

            return checkpointSeatCards;
        }

        private static SeatCards ToSeatCards(CheckpointSeatCards? checkpointSeatCards)
        {
            CheckpointSeatCards requiredSeatCards = Require(checkpointSeatCards);
            var seatCards = new SeatCards(requiredSeatCards.SeatNumber, Require(requiredSeatCards.Hand), Require(requiredSeatCards.Deck), requiredSeatCards.HasUsedPointsCard);

            return seatCards;
        }
    }
}
