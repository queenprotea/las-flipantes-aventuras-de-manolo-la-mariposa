using System;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class SessionTokenIssuerTests
    {
        private const int ShortKeyBytes = 16;
        private const int MatchId = 5;
        private const int NoPlayerId = 0;
        private const int PositiveGuestId = 3;
        private const string TamperedEnding = "x";
        private const string UnsignedToken = "esto-no-es-un-token";

        private static readonly TimeSpan _afterTheSessionLifetime = TimeSpan.FromMinutes(6);
        private static readonly TimeSpan _matchTokenLifetime = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan _afterTheMatchTokenLifetime = TimeSpan.FromMinutes(11);

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly SessionTokenIssuer _tokenIssuer;

        public SessionTokenIssuerTests()
        {
            _tokenIssuer = new SessionTokenIssuer(SampleServices.SigningKey, _fakeTimeProvider);
        }

        [Fact]
        public void Test_Constructor_WithAShortKey_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SessionTokenIssuer(new byte[ShortKeyBytes], _fakeTimeProvider));
        }

        [Fact]
        public void Test_Read_OfAFreshPlayerToken_ReturnsValid()
        {
            string sessionToken = _tokenIssuer.Issue(SamplePlayers.AnaId, SamplePlayers.AnaUsername);

            SessionTokenReading tokenReading = _tokenIssuer.Read(sessionToken);

            Assert.Equal(SessionTokenStatus.Valid, tokenReading.Status);
        }

        [Fact]
        public void Test_Read_OfAPlayerToken_ReturnsThePlayerId()
        {
            string sessionToken = _tokenIssuer.Issue(SamplePlayers.AnaId, SamplePlayers.AnaUsername);

            SessionTokenReading tokenReading = _tokenIssuer.Read(sessionToken);

            Assert.Equal(SamplePlayers.AnaId, tokenReading.OwnerId);
        }

        [Fact]
        public void Test_Read_OfAPlayerToken_ReturnsTheUsername()
        {
            string sessionToken = _tokenIssuer.Issue(SamplePlayers.AnaId, SamplePlayers.AnaUsername);

            SessionTokenReading tokenReading = _tokenIssuer.Read(sessionToken);

            Assert.Equal(SamplePlayers.AnaUsername, tokenReading.DisplayName);
        }

        [Fact]
        public void Test_Read_OfAGuestToken_MarksTheOwnerAsGuest()
        {
            string sessionToken = _tokenIssuer.IssueForGuest(SamplePlayers.CaroGuestId, SamplePlayers.CaroAlias);

            SessionTokenReading tokenReading = _tokenIssuer.Read(sessionToken);

            Assert.True(tokenReading.IsGuest);
        }

        [Fact]
        public void Test_Read_OfAGuestToken_ReturnsTheGuestId()
        {
            string sessionToken = _tokenIssuer.IssueForGuest(SamplePlayers.CaroGuestId, SamplePlayers.CaroAlias);

            SessionTokenReading tokenReading = _tokenIssuer.Read(sessionToken);

            Assert.Equal(SamplePlayers.CaroGuestId, tokenReading.OwnerId);
        }

        [Fact]
        public void Test_Read_AfterTheLifetimeOfTheToken_ReturnsExpired()
        {
            string sessionToken = _tokenIssuer.Issue(SamplePlayers.AnaId, SamplePlayers.AnaUsername);
            _fakeTimeProvider.Advance(_afterTheSessionLifetime);

            SessionTokenReading tokenReading = _tokenIssuer.Read(sessionToken);

            Assert.Equal(SessionTokenStatus.Expired, tokenReading.Status);
        }

        [Fact]
        public void Test_Read_OfATamperedToken_ReturnsInvalid()
        {
            string sessionToken = _tokenIssuer.Issue(SamplePlayers.AnaId, SamplePlayers.AnaUsername) + TamperedEnding;

            SessionTokenReading tokenReading = _tokenIssuer.Read(sessionToken);

            Assert.Equal(SessionTokenStatus.Invalid, tokenReading.Status);
        }

        [Fact]
        public void Test_Read_OfATextWithoutSignature_ReturnsInvalid()
        {
            SessionTokenReading tokenReading = _tokenIssuer.Read(UnsignedToken);

            Assert.Equal(SessionTokenStatus.Invalid, tokenReading.Status);
        }

        [Fact]
        public void Test_Read_OfATokenSignedWithAnotherKey_ReturnsInvalid()
        {
            var otherIssuer = new SessionTokenIssuer(CreateOtherKey(), _fakeTimeProvider);
            string sessionToken = otherIssuer.Issue(SamplePlayers.AnaId, SamplePlayers.AnaUsername);

            SessionTokenReading tokenReading = _tokenIssuer.Read(sessionToken);

            Assert.Equal(SessionTokenStatus.Invalid, tokenReading.Status);
        }

        [Fact]
        public void Test_Read_OfAMatchToken_ReturnsTheMatchId()
        {
            string matchToken = _tokenIssuer.IssueForMatch(MatchId, SamplePlayers.Ana, _matchTokenLifetime);

            SessionTokenReading tokenReading = _tokenIssuer.Read(matchToken);

            Assert.Equal(MatchId, tokenReading.MatchId);
        }

        [Fact]
        public void Test_Read_OfAMatchTokenOfAGuest_MarksTheOwnerAsGuest()
        {
            string matchToken = _tokenIssuer.IssueForMatch(MatchId, SamplePlayers.CaroGuest, _matchTokenLifetime);

            SessionTokenReading tokenReading = _tokenIssuer.Read(matchToken);

            Assert.True(tokenReading.IsGuest);
        }

        [Fact]
        public void Test_Read_AfterTheLifetimeOfAMatchToken_ReturnsExpired()
        {
            string matchToken = _tokenIssuer.IssueForMatch(MatchId, SamplePlayers.Ana, _matchTokenLifetime);
            _fakeTimeProvider.Advance(_afterTheMatchTokenLifetime);

            SessionTokenReading tokenReading = _tokenIssuer.Read(matchToken);

            Assert.Equal(SessionTokenStatus.Expired, tokenReading.Status);
        }

        [Fact]
        public void Test_Issue_WithoutPlayerId_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _tokenIssuer.Issue(NoPlayerId, SamplePlayers.AnaUsername));
        }

        [Fact]
        public void Test_IssueForGuest_WithAPositiveId_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _tokenIssuer.IssueForGuest(PositiveGuestId, SamplePlayers.CaroAlias));
        }

        [Fact]
        public void Test_IssueForMatch_WithoutLifetime_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _tokenIssuer.IssueForMatch(MatchId, SamplePlayers.Ana, TimeSpan.Zero));
        }

        private static byte[] CreateOtherKey()
        {
            var otherKey = new byte[SessionTokenIssuer.ShortestSigningKeyBytes];
            Array.Fill(otherKey, byte.MaxValue);

            return otherKey;
        }
    }
}
