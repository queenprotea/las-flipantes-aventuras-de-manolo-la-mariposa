using System;
using System.Threading.Tasks;

namespace Torres.Client.Session
{
    internal sealed class BackgroundWork<TArgument>
    {
        private readonly Action<TArgument> _work;
        private readonly TArgument _argument;

        internal BackgroundWork(Action<TArgument> work, TArgument argument)
        {
            _work = work;
            _argument = argument;
        }

        internal Task StartAsync()
        {
            Task runningWork = Task.Run(Invoke);

            return runningWork;
        }

        private void Invoke()
        {
            _work(_argument);
        }
    }
}
