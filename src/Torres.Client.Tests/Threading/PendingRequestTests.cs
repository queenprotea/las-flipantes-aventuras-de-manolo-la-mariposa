using System;
using System.Threading.Tasks;

using Torres.Client.Threading;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class PendingRequestTests
    {
        private const int ServerReply = 7;

        private readonly PendingRequest<int> _pendingRequest = new PendingRequest<int>();

        [Fact]
        public void Test_Start_WhileWaiting_ThrowsInvalidOperationException()
        {
            _pendingRequest.Start(new TaskCompletionSource<int>().Task);

            Assert.Throws<InvalidOperationException>(() => _pendingRequest.Start(Task.FromResult(ServerReply)));
        }

        [Fact]
        public void Test_TakeFinished_OfACompletedCall_ReturnsItsReply()
        {
            _pendingRequest.Start(Task.FromResult(ServerReply));

            FinishedRequest<int>? finishedRequest = _pendingRequest.TakeFinished();

            Assert.Equal(new FinishedRequest<int>(true, ServerReply), finishedRequest);
        }

        [Fact]
        public void Test_TakeFinished_OfAFailedCall_ReturnsItUnsuccessful()
        {
            _pendingRequest.Start(SampleClients.CreateFailedCallAsync<int>());

            FinishedRequest<int>? finishedRequest = _pendingRequest.TakeFinished();

            Assert.Equal(new FinishedRequest<int>(false, default), finishedRequest);
        }

        [Fact]
        public void Test_TakeFinished_WhileWaiting_ReturnsNull()
        {
            _pendingRequest.Start(new TaskCompletionSource<int>().Task);

            FinishedRequest<int>? finishedRequest = _pendingRequest.TakeFinished();

            Assert.Null(finishedRequest);
        }

        [Fact]
        public void Test_Forget_OfACallInProgress_StopsWaiting()
        {
            _pendingRequest.Start(new TaskCompletionSource<int>().Task);

            _pendingRequest.Forget();

            Assert.False(_pendingRequest.IsWaiting);
        }
    }
}
