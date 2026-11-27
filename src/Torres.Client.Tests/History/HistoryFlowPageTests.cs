using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class HistoryFlowPageTests
    {
        private const int SecondPage = 2;
        private const int ThreePages = 3;

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly HistoryFlow _historyFlow;

        public HistoryFlowPageTests()
        {
            _gameSessionConnection.GetMatchHistoryReply = Task.FromResult(new MatchHistoryResponse { PageNumber = SecondPage, PageCount = ThreePages });
            _historyFlow = new HistoryFlow(_gameSessionConnection, _playerSession);
        }

        [Fact]
        public void Test_Open_AsAGuest_ReturnsFalse()
        {
            _playerSession.StartAsGuest(SampleClients.GuestAlias);

            bool canOpen = _historyFlow.Open();

            Assert.False(canOpen);
        }

        [Fact]
        public void Test_TakeOutcome_AfterThePageArrives_ShowsItsNumber()
        {
            _historyFlow.Open();

            _historyFlow.TakeOutcome();

            int shownPageNumber = _historyFlow.ShownPageNumber;
            Assert.Equal(SecondPage, shownPageNumber);
        }

        [Fact]
        public void Test_HasSeveralPages_WithThreePages_ReturnsTrue()
        {
            LoadSecondPage();

            bool hasSeveralPages = _historyFlow.HasSeveralPages;

            Assert.True(hasSeveralPages);
        }

        [Fact]
        public void Test_CanGoToPreviousPage_OnTheSecondPage_ReturnsTrue()
        {
            LoadSecondPage();

            bool canGoBack = _historyFlow.CanGoToPreviousPage;

            Assert.True(canGoBack);
        }

        [Fact]
        public void Test_CanGoToNextPage_OnTheSecondOfThreePages_ReturnsTrue()
        {
            LoadSecondPage();

            bool canGoForward = _historyFlow.CanGoToNextPage;

            Assert.True(canGoForward);
        }

        [Fact]
        public void Test_GoToPreviousPage_WithTheSessionOpen_ReturnsTrue()
        {
            LoadSecondPage();

            bool isStarted = _historyFlow.GoToPreviousPage();

            Assert.True(isStarted);
        }

        [Fact]
        public void Test_GoToNextPage_WithTheSessionClosed_ReturnsFalse()
        {
            LoadSecondPage();
            _gameSessionConnection.IsSessionOpen = false;

            bool isStarted = _historyFlow.GoToNextPage();

            Assert.False(isStarted);
        }

        [Fact]
        public void Test_IsLoading_AfterOpening_ReturnsTrue()
        {
            _historyFlow.Open();

            bool isLoading = _historyFlow.IsLoading;

            Assert.True(isLoading);
        }

        private void LoadSecondPage()
        {
            _historyFlow.Open();
            _historyFlow.TakeOutcome();
        }
    }
}
