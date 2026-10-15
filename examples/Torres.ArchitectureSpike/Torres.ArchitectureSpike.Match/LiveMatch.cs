using System;
using System.Collections.Generic;

using Torres.ArchitectureSpike.Contracts;
using Torres.ArchitectureSpike.Domain;

namespace Torres.ArchitectureSpike.Match
{
    internal sealed class LiveMatch
    {
        private readonly object _turnLock = new object();
        private readonly string[] _playerNames;
        private readonly MatchEnvironment _environment;
        private readonly TokenRace _race;

        private IDisposable? _turnTimer;
        private int _turnNumber;

        internal LiveMatch(string[] playerNames, MatchEnvironment environment)
        {
            _playerNames = playerNames;
            _environment = environment;
            _race = new TokenRace(environment.Rules);
        }

        internal void Start()
        {
            List<PendingDelivery> deliveries;
            lock (_turnLock)
            {
                RestartTurnTimer();
                deliveries = CreateDeliveries(MatchEventCode.MatchStarted);
            }

            Deliver(deliveries);
        }

        internal ActionResult Advance(string playerName, int steps)
        {
            int seat = Array.IndexOf(_playerNames, playerName) + TokenRace.FirstSeat;
            var deliveries = new List<PendingDelivery>();
            StepOutcome stepOutcome;
            lock (_turnLock)
            {
                stepOutcome = _race.Advance(seat, steps);
                if (stepOutcome == StepOutcome.Applied)
                {
                    RestartTurnTimer();
                    deliveries = CreateDeliveries(ChooseStepEvent());
                }
            }

            Deliver(deliveries);
            ActionResult actionResult = MatchUpdateTranslator.ToActionResult(stepOutcome);

            return actionResult;
        }

        internal void ExpireTurn(int expiredTurnNumber)
        {
            var deliveries = new List<PendingDelivery>();
            lock (_turnLock)
            {
                if ((expiredTurnNumber == _turnNumber) && !_race.IsFinished)
                {
                    _race.ExpireTurn();
                    RestartTurnTimer();
                    deliveries = CreateDeliveries(MatchEventCode.TurnExpired);
                }
            }

            Deliver(deliveries);
        }

        private MatchEventCode ChooseStepEvent()
        {
            MatchEventCode stepEvent = _race.IsFinished ? MatchEventCode.MatchFinished : MatchEventCode.StepApplied;

            return stepEvent;
        }

        private void RestartTurnTimer()
        {
            _turnTimer?.Dispose();
            _turnTimer = null;
            _turnNumber++;
            if (_race.IsFinished)
            {
                return;
            }

            var turnExpiry = new TurnExpiry(this, _turnNumber);
            _turnTimer = _environment.Clock.Schedule(_environment.TurnDuration, turnExpiry.Expire);
        }

        private List<PendingDelivery> CreateDeliveries(MatchEventCode eventCode)
        {
            var deliveries = new List<PendingDelivery>();
            for (int seatIndex = 0; seatIndex < _playerNames.Length; seatIndex++)
            {
                int recipientSeat = seatIndex + TokenRace.FirstSeat;
                MatchUpdate update = MatchUpdateTranslator.ToUpdate(_race, eventCode, recipientSeat);
                deliveries.Add(new PendingDelivery(_playerNames[seatIndex], update));
            }

            return deliveries;
        }

        private void Deliver(List<PendingDelivery> deliveries)
        {
            foreach (PendingDelivery delivery in deliveries)
            {
                _environment.NotificationReceiver.Deliver(delivery.PlayerName, delivery.Update);
            }
        }
    }
}
