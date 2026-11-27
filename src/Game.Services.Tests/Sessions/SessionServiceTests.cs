using System;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class SessionServiceTests
    {
        private const int MatchId = 5;
        private const string UnsignedToken = "esto-no-es-un-token";

        private static readonly TimeSpan _afterTheSessionLifetime = TimeSpan.FromMinutes(6);
        private static readonly TimeSpan _matchTokenLifetime = TimeSpan.FromMinutes(10);

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly SessionTokenIssuer _tokenIssuer;
        private readonly SessionService _sessionService;

        public SessionServiceTests()
        {
            var roomRegistry = new RoomRegistry();
            RoomService roomService = SampleServices.CreateRoomService(_callerSessions, roomRegistry, _fakeTimeProvider);
            MatchService matchService = SampleServices.CreateMatchService(_callerSessions, roomRegistry, roomService, _fakeTimeProvider);
            AvatarUploadRegistry avatarUploadRegistry = SampleServices.CreateAvatarUploadRegistry(_callerSessions, _fakeTimeProvider);
            _tokenIssuer = new SessionTokenIssuer(SampleServices.SigningKey, _fakeTimeProvider);
            _sessionService = new SessionService(_callerSessions.Registry, _tokenIssuer, roomService, matchService, avatarUploadRegistry);
            _callerSessions.ActAs(SamplePlayers.Ana);
        }

        [Fact]
        public void Test_OpenSession_WithAnEmptyToken_ReturnsInvalidSessionToken()
        {
            OpenSessionResult opening = _sessionService.OpenSession(string.Empty);

            Assert.Equal(OpenSessionStatus.InvalidSessionToken, opening.Status);
        }

        [Fact]
        public void Test_OpenSession_WithAnUnsignedToken_ReturnsInvalidSessionToken()
        {
            OpenSessionResult opening = _sessionService.OpenSession(UnsignedToken);

            Assert.Equal(OpenSessionStatus.InvalidSessionToken, opening.Status);
        }

        [Fact]
        public void Test_OpenSession_WithAValidPlayerToken_ReturnsSessionOpened()
        {
            string sessionToken = _tokenIssuer.Issue(SamplePlayers.AnaId, SamplePlayers.AnaUsername);

            OpenSessionResult opening = _sessionService.OpenSession(sessionToken);

            Assert.Equal(OpenSessionStatus.SessionOpened, opening.Status);
        }

        [Fact]
        public void Test_OpenSession_WithAValidPlayerToken_ReturnsThePlayerId()
        {
            string sessionToken = _tokenIssuer.Issue(SamplePlayers.AnaId, SamplePlayers.AnaUsername);

            OpenSessionResult opening = _sessionService.OpenSession(sessionToken);

            Assert.Equal(SamplePlayers.AnaId, opening.Identity?.PlayerId);
        }

        [Fact]
        public void Test_OpenSession_WithAValidPlayerToken_RegistersTheSession()
        {
            string sessionToken = _tokenIssuer.Issue(SamplePlayers.AnaId, SamplePlayers.AnaUsername);

            _sessionService.OpenSession(sessionToken);

            bool hasOpenSession = _callerSessions.Registry.HasOpenSession(SamplePlayers.AnaId);
            Assert.True(hasOpenSession);
        }

        [Fact]
        public void Test_OpenSession_WithAValidGuestToken_MarksTheIdentityAsGuest()
        {
            string sessionToken = _tokenIssuer.IssueForGuest(SamplePlayers.CaroGuestId, SamplePlayers.CaroAlias);

            OpenSessionResult opening = _sessionService.OpenSession(sessionToken);

            bool? isGuest = opening.Identity?.IsGuest;
            Assert.True(isGuest);
        }

        [Fact]
        public void Test_OpenSession_WithAValidGuestToken_LeavesNoPlayerId()
        {
            string sessionToken = _tokenIssuer.IssueForGuest(SamplePlayers.CaroGuestId, SamplePlayers.CaroAlias);

            OpenSessionResult opening = _sessionService.OpenSession(sessionToken);

            Assert.Null(opening.Identity?.PlayerId);
        }

        [Fact]
        public void Test_OpenSession_WithAnExpiredToken_ReturnsSessionTokenExpired()
        {
            string sessionToken = _tokenIssuer.Issue(SamplePlayers.AnaId, SamplePlayers.AnaUsername);
            _fakeTimeProvider.Advance(_afterTheSessionLifetime);

            OpenSessionResult opening = _sessionService.OpenSession(sessionToken);

            Assert.Equal(OpenSessionStatus.SessionTokenExpired, opening.Status);
        }

        [Fact]
        public void Test_OpenSession_WithAMatchTokenOfASeatThatIsGone_ReturnsSessionTokenExpired()
        {
            string matchToken = _tokenIssuer.IssueForMatch(MatchId, SamplePlayers.Ana, _matchTokenLifetime);

            OpenSessionResult opening = _sessionService.OpenSession(matchToken);

            Assert.Equal(OpenSessionStatus.SessionTokenExpired, opening.Status);
        }

        [Fact]
        public async Task Test_CloseSessionAsync_WithoutSession_ReturnsSessionClosed()
        {
            CloseSessionResult closing = await _sessionService.CloseSessionAsync();

            Assert.Equal(CloseSessionResult.SessionClosed, closing);
        }

        [Fact]
        public async Task Test_CloseSessionAsync_WithAnOpenSession_ClosesIt()
        {
            _callerSessions.Open(SamplePlayers.Ana);

            await _sessionService.CloseSessionAsync();

            bool hasOpenSession = _callerSessions.Registry.HasOpenSession(SamplePlayers.AnaId);
            Assert.False(hasOpenSession);
        }
    }
}
