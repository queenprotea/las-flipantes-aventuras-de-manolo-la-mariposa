using System.ServiceModel;
using System.Threading.Tasks;

using Game.Contracts;

namespace Torres.Client.Connection
{
    internal sealed class ServerStatusConnection : IServerStatusConnection
    {
        private readonly ChannelFactory<IServerStatusService> _channelFactory;

        private IServerStatusService? _openChannel;

        internal ServerStatusConnection(ChannelFactory<IServerStatusService> channelFactory)
        {
            _channelFactory = channelFactory;
        }

        public Task<ServerStatus> GetStatusAsync()
        {
            ReleaseChannel();
            IServerStatusService statusChannel = _channelFactory.CreateChannel();
            _openChannel = statusChannel;
            Task<ServerStatus> statusCheck = statusChannel.GetStatusAsync();

            return statusCheck;
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
