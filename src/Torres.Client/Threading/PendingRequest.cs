using System;
using System.Threading.Tasks;

namespace Torres.Client.Threading
{
    internal sealed class PendingRequest<TReply>
    {
        private const string RequestInProgressMessage = "A request of this kind is already in progress.";

        private Task<TReply>? _request;

        internal bool IsWaiting => _request is not null;

        internal void Start(Task<TReply> request)
        {
            if (_request is not null)
            {
                throw new InvalidOperationException(RequestInProgressMessage);
            }

            _request = request;
        }

        internal FinishedRequest<TReply>? TakeFinished()
        {
            FinishedRequest<TReply>? finishedRequest = null;
            if ((_request is not null) && _request.IsCompleted)
            {
                finishedRequest = _request.IsCompletedSuccessfully
                    ? new FinishedRequest<TReply>(true, _request.Result)
                    : new FinishedRequest<TReply>(false, default);
                _request = null;
            }

            return finishedRequest;
        }

        internal void Forget()
        {
            _request = null;
        }
    }
}
