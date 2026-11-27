using System.ServiceModel;
using System.Threading.Tasks;

using Game.Contracts;

namespace Torres.Client.Connection
{
    internal sealed class RankingConnection : IRankingConnection
    {
        private readonly ChannelFactory<IRankingService> _channelFactory;

        private IRankingService? _openChannel;

        internal RankingConnection(ChannelFactory<IRankingService> channelFactory)
        {
            _channelFactory = channelFactory;
        }

        public Task<GlobalRankingResponse> GetGlobalRankingAsync()
        {
            ReleaseChannel();
            IRankingService rankingChannel = _channelFactory.CreateChannel();
            _openChannel = rankingChannel;
            Task<GlobalRankingResponse> rankingRequest = rankingChannel.GetGlobalRankingAsync();

            return rankingRequest;
        }

        public void ReleaseChannel()
        {
            if (_openChannel is ICommunicationObject usedChannel)
            {
                usedChannel.Abort();
            }

            _openChannel = null;
        }
    }
}
