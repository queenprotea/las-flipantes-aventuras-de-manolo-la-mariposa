using Torres.ArchitectureSpike.Contracts;

namespace Torres.ArchitectureSpike.Services
{
    internal sealed class SessionDelivery
    {
        private readonly MatchUpdate? _matchUpdate;
        private readonly PlayerConnectionNotice? _connectionNotice;

        private SessionDelivery(MatchUpdate? matchUpdate, PlayerConnectionNotice? connectionNotice)
        {
            _matchUpdate = matchUpdate;
            _connectionNotice = connectionNotice;
        }

        internal static SessionDelivery ForMatchUpdate(MatchUpdate matchUpdate)
        {
            var delivery = new SessionDelivery(matchUpdate, null);

            return delivery;
        }

        internal static SessionDelivery ForConnectionNotice(PlayerConnectionNotice connectionNotice)
        {
            var delivery = new SessionDelivery(null, connectionNotice);

            return delivery;
        }

        internal void SendTo(IGameSessionCallback callbackChannel)
        {
            if (_matchUpdate is not null)
            {
                callbackChannel.MatchUpdated(_matchUpdate);
            }
            else if (_connectionNotice is not null)
            {
                callbackChannel.PlayerConnectionChanged(_connectionNotice);
            }
        }
    }
}
