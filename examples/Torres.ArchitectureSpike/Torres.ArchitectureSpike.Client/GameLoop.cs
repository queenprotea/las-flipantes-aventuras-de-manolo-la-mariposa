using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Torres.ArchitectureSpike.Client
{
    internal sealed class GameLoop
    {
        private const int FrameMilliseconds = 16;

        private readonly object _clientsLock = new object();
        private readonly List<SpikeClient> _clients = new List<SpikeClient>();
        private readonly ManualResetEventSlim _loopStarted = new ManualResetEventSlim(false);

        private volatile bool _isRunning;
        private Thread? _loopThread;

        internal int LoopThreadId { get; private set; }

        internal void Add(SpikeClient client)
        {
            lock (_clientsLock)
            {
                _clients.Add(client);
            }
        }

        internal void Start()
        {
            _isRunning = true;
            _loopThread = new Thread(RunFrames) { IsBackground = true };
            _loopThread.Start();
            _loopStarted.Wait();
        }

        internal void Stop()
        {
            _isRunning = false;
            _loopThread?.Join();
        }

        private void RunFrames()
        {
            LoopThreadId = Environment.CurrentManagedThreadId;
            _loopStarted.Set();
            while (_isRunning)
            {
                ProcessFrame();
                Thread.Sleep(FrameMilliseconds);
            }
        }

        private void ProcessFrame()
        {
            List<SpikeClient> frameClients;
            lock (_clientsLock)
            {
                frameClients = _clients.ToList();
            }

            foreach (SpikeClient client in frameClients)
            {
                client.DrainArrivals(LoopThreadId);
            }
        }
    }
}
