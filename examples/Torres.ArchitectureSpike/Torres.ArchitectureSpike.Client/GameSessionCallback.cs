using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;

using Torres.ArchitectureSpike.Contracts;

namespace Torres.ArchitectureSpike.Client
{
    [CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public sealed class GameSessionCallback : IGameSessionCallback
    {
        private readonly ConcurrentQueue<ReceivedNotice> _arrivals = new ConcurrentQueue<ReceivedNotice>();

        public void PlayerConnectionChanged(PlayerConnectionNotice notice)
        {
            _arrivals.Enqueue(new ReceivedNotice(null, notice));
        }

        public void MatchUpdated(MatchUpdate update)
        {
            _arrivals.Enqueue(new ReceivedNotice(update, null));
        }

        internal bool TryTakeArrival([NotNullWhen(true)] out ReceivedNotice? arrival)
        {
            bool isTaken = _arrivals.TryDequeue(out arrival);

            return isTaken;
        }
    }
}
