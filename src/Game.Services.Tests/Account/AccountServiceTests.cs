using System;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Persistence;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class AccountServiceTests
    {
        private const string GuestAlias = "Lupita";
        private const string BlankAlias = "   ";
        private const string WrongPassword = "Otra#2026x";
        private const string UnknownUsername = "nadie_aqui";
        private const int CodeLifetimeMinutes = 10;

        private static readonly byte[] _signingKey = new byte[SessionTokenIssuer.ShortestSigningKeyBytes];
        private static readonly TimeSpan _temporaryBanLength = TimeSpan.FromDays(1);

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly RecordingPlayerRepository _playerRepository = new RecordingPlayerRepository();
        private readonly RecordingSanctionRepository _sanctionRepository = new RecordingSanctionRepository();
        private readonly AccountService _accountService;

        public AccountServiceTests()
        {
            var tokenIssuer = new SessionTokenIssuer(_signingKey, _fakeTimeProvider);
            var emailSender = new RecordingEmailSender();
            var accountRegistration = new AccountRegistration(_playerRepository, emailSender, new PendingRegistrationRegistry(_fakeTimeProvider), tokenIssuer, _fakeTimeProvider);
            var accessRecovery = new AccessRecovery(_playerRepository, new RecordingPasswordRecoveryRepository(), emailSender, _fakeTimeProvider);
            accountRegistration.Start(CodeLifetimeMinutes);
            accessRecovery.Start(CodeLifetimeMinutes);
            _accountService = new AccountService(_playerRepository, _sanctionRepository, tokenIssuer, new GuestIdIssuer(), accountRegistration, accessRecovery);
        }

        [Fact]
        public async Task Test_LoginAsync_WithCorrectCredentials_ReturnsLoggedIn()
        {
            LoginResult loginResult = await _accountService.LoginAsync(SamplePlayers.AnaUsername, SamplePlayers.Password);

            Assert.Equal(LoginStatus.LoggedIn, loginResult.Status);
        }

        [Fact]
        public async Task Test_LoginAsync_WithCorrectCredentials_ReturnsThePlayerIdentity()
        {
            LoginResult loginResult = await _accountService.LoginAsync(SamplePlayers.AnaUsername, SamplePlayers.Password);

            Assert.Equal(SamplePlayers.AnaId, loginResult.Player?.PlayerId);
        }

        [Fact]
        public async Task Test_LoginAsync_WithWrongPassword_ReturnsInvalidCredentials()
        {
            LoginResult loginResult = await _accountService.LoginAsync(SamplePlayers.AnaUsername, WrongPassword);

            Assert.Equal(LoginStatus.InvalidCredentials, loginResult.Status);
        }

        [Fact]
        public async Task Test_LoginAsync_WithUnknownUsername_ReturnsInvalidCredentials()
        {
            LoginResult loginResult = await _accountService.LoginAsync(UnknownUsername, SamplePlayers.Password);

            Assert.Equal(LoginStatus.InvalidCredentials, loginResult.Status);
        }

        [Fact]
        public async Task Test_LoginAsync_WithPermanentBan_ReturnsAccountBanned()
        {
            _sanctionRepository.Sanctions.Add(new Sanction { PlayerId = SamplePlayers.AnaId, IsPermanent = true, EffectiveFrom = _fakeTimeProvider.GetUtcNow().UtcDateTime });

            LoginResult loginResult = await _accountService.LoginAsync(SamplePlayers.AnaUsername, SamplePlayers.Password);

            Assert.Equal(LoginStatus.AccountBanned, loginResult.Status);
        }

        [Fact]
        public async Task Test_LoginAsync_WithTemporaryBan_ReturnsLoggedIn()
        {
            DateTime nowUtc = _fakeTimeProvider.GetUtcNow().UtcDateTime;
            _sanctionRepository.Sanctions.Add(new Sanction { PlayerId = SamplePlayers.AnaId, IsPermanent = false, EffectiveFrom = nowUtc, EffectiveUntil = nowUtc + _temporaryBanLength });

            LoginResult loginResult = await _accountService.LoginAsync(SamplePlayers.AnaUsername, SamplePlayers.Password);

            Assert.Equal(LoginStatus.LoggedIn, loginResult.Status);
        }

        [Fact]
        public void Test_EnterAsGuest_WithAlias_ReturnsEntered()
        {
            GuestAccessResult accessResult = _accountService.EnterAsGuest(GuestAlias);

            Assert.Equal(GuestAccessStatus.Entered, accessResult.Status);
        }

        [Fact]
        public void Test_EnterAsGuest_WithAlias_ReturnsASessionToken()
        {
            GuestAccessResult accessResult = _accountService.EnterAsGuest(GuestAlias);

            Assert.NotNull(accessResult.SessionToken);
        }

        [Fact]
        public void Test_EnterAsGuest_WithBlankAlias_ReturnsAliasRequired()
        {
            GuestAccessResult accessResult = _accountService.EnterAsGuest(BlankAlias);

            Assert.Equal(GuestAccessStatus.AliasRequired, accessResult.Status);
        }
    }
}
