using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public sealed class ActionCardTable
    {
        public const int SixActionPointsTotal = 6;
        public const int SevenActionPointsTotal = 7;
        public const int PointsPerKeptCard = 1;

        private readonly MatchTable _matchTable;
        private readonly ActionCardEffects _cardEffects;
        private readonly Random _chanceSource;
        private readonly List<SeatCards> _seatCards;

        internal ActionCardTable(MatchTable matchTable, Board board, IReadOnlyList<SeatCards> seatCards, int seatCount, Random chanceSource)
        {
            _matchTable = matchTable;
            _cardEffects = new ActionCardEffects(matchTable, board);
            _chanceSource = chanceSource;
            _seatCards = seatCards.Count == 0 ? CreateFreshSeatCards(seatCount) : seatCards.ToList();
        }

        public static IReadOnlyList<ActionCard> FullDeck { get; } = Enum.GetValues<ActionCard>();

        public IReadOnlyList<SeatCards> SeatCards => _seatCards;

        public TurnActionOutcome DrawCard(int seatNumber)
        {
            TurnActionOutcome actionOutcome = _matchTable.JudgeTurnOwner(seatNumber);
            if (actionOutcome == TurnActionOutcome.Applied)
            {
                actionOutcome = JudgeDraw(GetSeatCards(seatNumber));
            }

            if (actionOutcome == TurnActionOutcome.Applied)
            {
                MoveRandomCardToHand(seatNumber);
                _matchTable.SpendActionPoints(_matchTable.ActionCosts.CardDraw);
            }

            return actionOutcome;
        }

        public TurnActionOutcome PlayCard(int seatNumber, CardPlay cardPlay)
        {
            TurnActionOutcome actionOutcome = _matchTable.JudgeTurnOwner(seatNumber);
            if (actionOutcome == TurnActionOutcome.Applied)
            {
                actionOutcome = JudgeHeldCard(GetSeatCards(seatNumber), cardPlay.Card);
            }

            if (actionOutcome == TurnActionOutcome.Applied)
            {
                actionOutcome = ApplyCard(seatNumber, cardPlay);
            }

            if (actionOutcome == TurnActionOutcome.Applied)
            {
                RemoveFromHand(seatNumber, cardPlay.Card);
                _matchTable.SpendActionPoints(CalculatePlayCost(cardPlay.Card));
            }

            return actionOutcome;
        }

        public bool CanDrawCard(int seatNumber)
        {
            bool canDraw = (_matchTable.JudgeTurnOwner(seatNumber) == TurnActionOutcome.Applied) && (JudgeDraw(GetSeatCards(seatNumber)) == TurnActionOutcome.Applied);

            return canDraw;
        }

        public IReadOnlyList<ActionCard> FindPlayableCards(int seatNumber)
        {
            SeatCards seatCards = GetSeatCards(seatNumber);
            bool isTurnOwner = _matchTable.JudgeTurnOwner(seatNumber) == TurnActionOutcome.Applied;
            List<ActionCard> playableCards = isTurnOwner ? seatCards.Hand.Where(handCard => JudgeHeldCard(seatCards, handCard) == TurnActionOutcome.Applied).ToList() : new List<ActionCard>();

            return playableCards;
        }

        public IReadOnlyList<BoardCell> FindLevelRemovalTargets()
        {
            IReadOnlyList<BoardCell> removalTargets = _cardEffects.FindLevelRemovalTargets();

            return removalTargets;
        }

        internal void StartTurn()
        {
            for (var i = 0; i < _seatCards.Count; i++)
            {
                _seatCards[i] = _seatCards[i] with { HasUsedPointsCard = false };
            }
        }

        internal void CloseRound()
        {
            CashInHands();
            for (var i = 0; i < _seatCards.Count; i++)
            {
                _seatCards[i] = _seatCards[i] with { Deck = FullDeck, HasUsedPointsCard = false };
            }
        }

        internal void CashInHands()
        {
            for (var i = 0; i < _seatCards.Count; i++)
            {
                SeatCards seatCards = _seatCards[i];
                if (seatCards.Hand.Count > 0)
                {
                    _matchTable.AddCardPoints(seatCards.SeatNumber, seatCards.Hand.Count * PointsPerKeptCard);
                    _seatCards[i] = seatCards with { Hand = Array.Empty<ActionCard>() };
                }
            }
        }

        private static List<SeatCards> CreateFreshSeatCards(int seatCount)
        {
            List<SeatCards> freshSeatCards = Enumerable.Range(MatchTable.FirstSeatNumber, seatCount).Select(CreateFreshSeatCard).ToList();

            return freshSeatCards;
        }

        private static SeatCards CreateFreshSeatCard(int seatNumber)
        {
            var freshSeatCard = new SeatCards(seatNumber, Array.Empty<ActionCard>(), FullDeck, false);

            return freshSeatCard;
        }

        private static int ToSeatIndex(int seatNumber)
        {
            int seatIndex = seatNumber - MatchTable.FirstSeatNumber;

            return seatIndex;
        }

        private static bool IsPointsCard(ActionCard card)
        {
            bool isPointsCard = card is ActionCard.SixActionPoints or ActionCard.SevenActionPoints;

            return isPointsCard;
        }

        private TurnActionOutcome JudgeDraw(SeatCards seatCards)
        {
            TurnActionOutcome actionOutcome = TurnActionOutcome.Applied;
            if (seatCards.Deck.Count == 0)
            {
                actionOutcome = TurnActionOutcome.DeckEmpty;
            }
            else if (_matchTable.ActionPointsLeft < _matchTable.ActionCosts.CardDraw)
            {
                actionOutcome = TurnActionOutcome.NotEnoughActionPoints;
            }

            return actionOutcome;
        }

        private TurnActionOutcome JudgeHeldCard(SeatCards seatCards, ActionCard card)
        {
            TurnActionOutcome actionOutcome = TurnActionOutcome.Applied;
            if (!seatCards.Hand.Contains(card))
            {
                actionOutcome = TurnActionOutcome.CardNotInHand;
            }
            else if (IsPointsCard(card) && seatCards.HasUsedPointsCard)
            {
                actionOutcome = TurnActionOutcome.PointsCardAlreadyUsed;
            }
            else if (_matchTable.ActionPointsLeft < CalculatePlayCost(card))
            {
                actionOutcome = TurnActionOutcome.NotEnoughActionPoints;
            }

            return actionOutcome;
        }

        private TurnActionOutcome ApplyCard(int seatNumber, CardPlay cardPlay)
        {
            TurnActionOutcome actionOutcome;
            if (IsPointsCard(cardPlay.Card))
            {
                actionOutcome = ApplyPointsCard(seatNumber, cardPlay);
            }
            else
            {
                actionOutcome = _cardEffects.Apply(seatNumber, cardPlay);
            }

            return actionOutcome;
        }

        private TurnActionOutcome ApplyPointsCard(int seatNumber, CardPlay cardPlay)
        {
            TurnActionOutcome actionOutcome = TurnActionOutcome.WrongCardTarget;
            if (cardPlay.AffectedCells.Count == 0)
            {
                int turnPointsTotal = cardPlay.Card == ActionCard.SixActionPoints ? SixActionPointsTotal : SevenActionPointsTotal;
                _matchTable.GrantActionPoints(turnPointsTotal - MatchTable.ActionPointsPerTurn);
                int seatIndex = ToSeatIndex(seatNumber);
                _seatCards[seatIndex] = _seatCards[seatIndex] with { HasUsedPointsCard = true };
                actionOutcome = TurnActionOutcome.Applied;
            }

            return actionOutcome;
        }

        private int CalculatePlayCost(ActionCard card)
        {
            int playCost = _matchTable.ActionCosts.CardPlay;
            if (card == ActionCard.PlanterCrossing)
            {
                playCost += _matchTable.ActionCosts.Movement;
            }

            return playCost;
        }

        private void MoveRandomCardToHand(int seatNumber)
        {
            int seatIndex = ToSeatIndex(seatNumber);
            SeatCards seatCards = _seatCards[seatIndex];
            ActionCard drawnCard = seatCards.Deck[_chanceSource.Next(seatCards.Deck.Count)];
            List<ActionCard> remainingDeck = seatCards.Deck.ToList();
            remainingDeck.Remove(drawnCard);
            _seatCards[seatIndex] = seatCards with { Hand = seatCards.Hand.Append(drawnCard).ToList(), Deck = remainingDeck };
        }

        private void RemoveFromHand(int seatNumber, ActionCard playedCard)
        {
            int seatIndex = ToSeatIndex(seatNumber);
            SeatCards seatCards = _seatCards[seatIndex];
            List<ActionCard> remainingHand = seatCards.Hand.ToList();
            remainingHand.Remove(playedCard);
            _seatCards[seatIndex] = seatCards with { Hand = remainingHand };
        }

        private SeatCards GetSeatCards(int seatNumber)
        {
            SeatCards seatCards = _seatCards[ToSeatIndex(seatNumber)];

            return seatCards;
        }
    }
}
