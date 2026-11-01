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
    }
}
