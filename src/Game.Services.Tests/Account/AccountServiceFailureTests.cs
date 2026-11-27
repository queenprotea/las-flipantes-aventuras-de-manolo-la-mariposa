using System;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class AccountServiceFailureTests
    {
        private const string NewUsername = "lupe_flores";
        private const string NewEmail = "lupe@torres.mx";
        private const string HeldCode = "482915";
        private const int CodeLifetimeMinutes = 10;

        private static readonly TimeSpan _codeLifetime = TimeSpan.FromMinutes(CodeLifetimeMinutes);

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly PendingRegistrationRegistry _pendingRegistrationRegistry;
        private readonly AccountService _accountService;

        public AccountServiceFailureTests()
        {
            var playerRepository = new FailingPlayerRepository();
            var emailSender = new RecordingEmailSender();
            var tokenIssuer = new SessionTokenIssuer(SampleServices.SigningKey, _fakeTimeProvider);
            _pendingRegistrationRegistry = new PendingRegistrationRegistry(_fakeTimeProvider);
            var verificationCodeLifetime = new VerificationCodeLifetime();
            verificationCodeLifetime.Start(CodeLifetimeMinutes);
            var accountRegistration = new AccountRegistration(playerRepository, emailSender, _pendingRegistrationRegistry, tokenIssuer, _fakeTimeProvider, verificationCodeLifetime);
            var accessRecovery = new AccessRecovery(playerRepository, new RecordingPasswordRecoveryRepository(), emailSender, _fakeTimeProvider, verificationCodeLifetime);
            _accountService = new AccountService(playerRepository, new RecordingSanctionRepository(), tokenIssuer, new GuestIdIssuer(), accountRegistration, accessRecovery);
        }

        [Fact]
        public async Task Test_LoginAsync_WithTheDatabaseDown_ReturnsDatabaseUnavailable()
        {
            LoginResult loginResult = await _accountService.LoginAsync(SamplePlayers.AnaUsername, SamplePlayers.Password);

            Assert.Equal(LoginStatus.DatabaseUnavailable, loginResult.Status);
        }

        [Fact]
        public async Task Test_RequestRegistrationCodeAsync_WithTheDatabaseDown_ReturnsDatabaseUnavailable()
        {
            RegistrationResult requestResult = await _accountService.RequestRegistrationCodeAsync(NewUsername, NewEmail, SamplePlayers.Password);

            Assert.Equal(RegistrationResult.DatabaseUnavailable, requestResult);
        }

        [Fact]
        public async Task Test_ConfirmRegistrationAsync_WithTheDatabaseDown_ReturnsDatabaseUnavailable()
        {
            DateTimeOffset expiresAt = _fakeTimeProvider.GetUtcNow() + _codeLifetime;
            _pendingRegistrationRegistry.Hold(new PendingRegistration(NewUsername, NewEmail, SamplePlayers.PasswordHash, HeldCode, expiresAt));

            RegistrationConfirmationResult confirmation = await _accountService.ConfirmRegistrationAsync(NewEmail, HeldCode);

            Assert.Equal(RegistrationConfirmationStatus.DatabaseUnavailable, confirmation.Status);
        }

        [Fact]
        public async Task Test_RequestRecoveryCodeAsync_WithTheDatabaseDown_ReturnsDatabaseUnavailable()
        {
            RecoveryCodeRequestResult requestResult = await _accountService.RequestRecoveryCodeAsync(SamplePlayers.AnaEmail);

            Assert.Equal(RecoveryCodeRequestResult.DatabaseUnavailable, requestResult);
        }

        [Fact]
        public async Task Test_VerifyRecoveryCodeAsync_WithTheDatabaseDown_ReturnsDatabaseUnavailable()
        {
            RecoveryCodeVerificationResult verification = await _accountService.VerifyRecoveryCodeAsync(SamplePlayers.AnaEmail, HeldCode);

            Assert.Equal(RecoveryCodeVerificationResult.DatabaseUnavailable, verification);
        }

        [Fact]
        public async Task Test_ResetPasswordAsync_WithTheDatabaseDown_ReturnsDatabaseUnavailable()
        {
            PasswordResetResult resetResult = await _accountService.ResetPasswordAsync(SamplePlayers.AnaEmail, HeldCode, SamplePlayers.Password);

            Assert.Equal(PasswordResetResult.DatabaseUnavailable, resetResult);
        }
    }
}
