using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

using Torres.ArchitectureSpike.Contracts;

namespace Torres.ArchitectureSpike.Client
{
    internal sealed class SpikeClient
    {
        private const int PollMilliseconds = 20;

        private readonly object _noticesLock = new object();
        private readonly List<ReceivedNotice> _unclaimedNotices = new List<ReceivedNotice>();
        private readonly List<ReceivedNotice> _processedHistory = new List<ReceivedNotice>();
        private readonly GameSessionCallback _callback = new GameSessionCallback();
        private readonly DuplexChannelFactory<IGameSessionService> _channelFactory;

        internal SpikeClient(string playerName, EndpointAddress serviceAddress)
        {
            PlayerName = playerName;
            var callbackContext = new InstanceContext(_callback);
            var sessionBinding = new NetTcpBinding(SecurityMode.None);
            _channelFactory = new DuplexChannelFactory<IGameSessionService>(callbackContext, sessionBinding, serviceAddress);
            Session = _channelFactory.CreateChannel();
        }

        internal string PlayerName { get; }

        internal IGameSessionService Session { get; }

        internal void DrainArrivals(int loopThreadId)
        {
            while (_callback.TryTakeArrival(out ReceivedNotice? arrival))
            {
                arrival.MarkProcessed(loopThreadId);
                lock (_noticesLock)
                {
                    _unclaimedNotices.Add(arrival);
                    _processedHistory.Add(arrival);
                }
            }
        }

        internal List<ReceivedNotice> CopyProcessedHistory()
        {
            List<ReceivedNotice> historyCopy;
            lock (_noticesLock)
            {
                historyCopy = _processedHistory.ToList();
            }

            return historyCopy;
        }

        internal async Task<ReceivedNotice?> WaitForAsync(NoticeQuery query, TimeSpan timeout)
        {
            Stopwatch waitWatch = Stopwatch.StartNew();
            ReceivedNotice? claimedNotice = ClaimFirst(query);
            while ((claimedNotice is null) && (waitWatch.Elapsed < timeout))
            {
                await Task.Delay(PollMilliseconds);
                claimedNotice = ClaimFirst(query);
            }

            return claimedNotice;
        }

        internal void Close()
        {
            ((ICommunicationObject)Session).Close();
            _channelFactory.Close();
        }

        internal void Abort()
        {
            ((ICommunicationObject)Session).Abort();
            _channelFactory.Abort();
        }

        private ReceivedNotice? ClaimFirst(NoticeQuery query)
        {
            ReceivedNotice? claimedNotice;
            lock (_noticesLock)
            {
                claimedNotice = _unclaimedNotices.FirstOrDefault(query.Matches);
                if (claimedNotice is not null)
                {
                    _unclaimedNotices.Remove(claimedNotice);
                }
            }

            return claimedNotice;
        }
    }
}
