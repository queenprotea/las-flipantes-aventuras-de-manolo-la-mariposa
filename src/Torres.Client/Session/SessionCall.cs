using System;
using System.Threading.Tasks;

namespace Torres.Client.Session
{
    internal sealed class SessionCall<TArgument, TResult>
    {
        private readonly Func<TArgument, TResult> _operation;
        private readonly TArgument _argument;

        internal SessionCall(Func<TArgument, TResult> operation, TArgument argument)
        {
            _operation = operation;
            _argument = argument;
        }

        internal Task<TResult> StartAsync()
        {
            Task<TResult> runningCall = Task.Run(Invoke);

            return runningCall;
        }

        private TResult Invoke()
        {
            TResult callResult = _operation(_argument);

            return callResult;
        }
    }
}
