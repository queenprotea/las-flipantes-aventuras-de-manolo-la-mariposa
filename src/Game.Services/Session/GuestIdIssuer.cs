using System.Threading;

namespace Game.Services
{
    public sealed class GuestIdIssuer
    {
        private int _lastGuestId;

        internal int Issue()
        {
            int guestId = Interlocked.Decrement(ref _lastGuestId);

            return guestId;
        }

        internal void SkipPast(int usedGuestId)
        {
            int lastGuestId = Volatile.Read(ref _lastGuestId);
            while ((usedGuestId < lastGuestId) && (Interlocked.CompareExchange(ref _lastGuestId, usedGuestId, lastGuestId) != lastGuestId))
            {
                lastGuestId = Volatile.Read(ref _lastGuestId);
            }
        }
    }
}
