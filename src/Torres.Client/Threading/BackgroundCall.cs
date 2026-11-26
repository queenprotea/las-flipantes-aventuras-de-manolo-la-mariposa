using System;
using System.Threading.Tasks;

namespace Torres.Client.Threading
{
    internal sealed class BackgroundCall<TArgument, TResult>
    {
        private readonly Func<TArgument, TResult> _operation;
        private readonly TArgument _argument;

        internal BackgroundCall(Func<TArgument, TResult> operation, TArgument argument)
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
