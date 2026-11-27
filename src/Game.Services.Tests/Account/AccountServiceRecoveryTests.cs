using System;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Persistence;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class AccountServiceRecoveryTests
    {
        private const string StoredCode = "731406";
        private const string WrongCode = "000000";
        private const string NewPassword = "Nueva#2027";
        private const string WeakPassword = "abc";
        private const string BadEmail = "ana-sin-arroba";
        private const string UnknownEmail = "nadie@torres.mx";
        private const int CodeLifetimeMinutes = 10;

        private static readonly byte[] _signingKey = new byte[SessionTokenIssuer.ShortestSigningKeyBytes];
        private static readonly TimeSpan _codeLifetime = TimeSpan.FromMinutes(CodeLifetimeMinutes);

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly RecordingPlayerRepository _playerRepository = new RecordingPlayerRepository();
        private readonly RecordingPasswordRecoveryRepository _recoveryRepository = new RecordingPasswordRecoveryRepository();
        private readonly RecordingEmailSender _emailSender = new RecordingEmailSender();
        private readonly AccountService _accountService;

        public AccountServiceRecoveryTests()
        {
            var tokenIssuer = new SessionTokenIssuer(_signingKey, _fakeTimeProvider);
            var verificationCodeLifetime = new VerificationCodeLifetime();
            verificationCodeLifetime.Start(CodeLifetimeMinutes);
            var accountRegistration = new AccountRegistration(_playerRepository, _emailSender, new PendingRegistrationRegistry(_fakeTimeProvider), tokenIssuer, _fakeTimeProvider, verificationCodeLifetime);
            var accessRecovery = new AccessRecovery(_playerRepository, _recoveryRepository, _emailSender, _fakeTimeProvider, verificationCodeLifetime);
            _accountService = new AccountService(_playerRepository, new RecordingSanctionRepository(), tokenIssuer, new GuestIdIssuer(), accountRegistration, accessRecovery);
        }

        [Fact]
        public async Task Test_RequestRecoveryCodeAsync_WithRegisteredEmail_ReturnsCodeSent()
        {
            RecoveryCodeRequestResult requestResult = await _accountService.RequestRecoveryCodeAsync(SamplePlayers.AnaEmail);

            Assert.Equal(RecoveryCodeRequestResult.CodeSent, requestResult);
        }

        [Fact]
        public async Task Test_RequestRecoveryCodeAsync_WithRegisteredEmail_StoresACodeForThePlayer()
        {
            await _accountService.RequestRecoveryCodeAsync(SamplePlayers.AnaEmail);

            Assert.Contains(SamplePlayers.AnaId, _recoveryRepository.RecoveriesByPlayerId.Keys);
        }

        [Fact]
        public async Task Test_VerifyRecoveryCodeAsync_WithStoredCode_ReturnsValid()
        {
            StoreRecoveryCode();

            RecoveryCodeVerificationResult verification = await _accountService.VerifyRecoveryCodeAsync(SamplePlayers.AnaEmail, StoredCode);

            Assert.Equal(RecoveryCodeVerificationResult.Valid, verification);
        }

        [Fact]
        public async Task Test_ResetPasswordAsync_WithStoredCode_ReturnsReset()
        {
            StoreRecoveryCode();

            PasswordResetResult resetResult = await _accountService.ResetPasswordAsync(SamplePlayers.AnaEmail, StoredCode, NewPassword);

            Assert.Equal(PasswordResetResult.Reset, resetResult);
        }

        [Fact]
        public async Task Test_RequestRecoveryCodeAsync_WithBadEmailFormat_ReturnsInvalidEmail()
        {
            RecoveryCodeRequestResult requestResult = await _accountService.RequestRecoveryCodeAsync(BadEmail);

            Assert.Equal(RecoveryCodeRequestResult.InvalidEmail, requestResult);
        }

        [Fact]
        public async Task Test_RequestRecoveryCodeAsync_WithUnknownEmail_ReturnsCodeSent()
        {
            RecoveryCodeRequestResult requestResult = await _accountService.RequestRecoveryCodeAsync(UnknownEmail);

            Assert.Equal(RecoveryCodeRequestResult.CodeSent, requestResult);
        }

        [Fact]
        public async Task Test_RequestRecoveryCodeAsync_WithUnknownEmail_SendsNoEmail()
        {
            await _accountService.RequestRecoveryCodeAsync(UnknownEmail);

            Assert.Empty(_emailSender.SentMessages);
        }

        [Fact]
        public async Task Test_VerifyRecoveryCodeAsync_WithWrongCode_ReturnsInvalidCode()
        {
            StoreRecoveryCode();

            RecoveryCodeVerificationResult verification = await _accountService.VerifyRecoveryCodeAsync(SamplePlayers.AnaEmail, WrongCode);

            Assert.Equal(RecoveryCodeVerificationResult.InvalidCode, verification);
        }

        [Fact]
        public async Task Test_VerifyRecoveryCodeAsync_WithExpiredCode_ReturnsInvalidCode()
        {
            StoreRecoveryCode();
            _fakeTimeProvider.Advance(_codeLifetime);

            RecoveryCodeVerificationResult verification = await _accountService.VerifyRecoveryCodeAsync(SamplePlayers.AnaEmail, StoredCode);

            Assert.Equal(RecoveryCodeVerificationResult.InvalidCode, verification);
        }

        [Fact]
        public async Task Test_ResetPasswordAsync_WithUsedCode_ReturnsInvalidCode()
        {
            StoreRecoveryCode();
            await _accountService.ResetPasswordAsync(SamplePlayers.AnaEmail, StoredCode, NewPassword);

            PasswordResetResult secondReset = await _accountService.ResetPasswordAsync(SamplePlayers.AnaEmail, StoredCode, NewPassword);

            Assert.Equal(PasswordResetResult.InvalidCode, secondReset);
        }

        [Fact]
        public async Task Test_ResetPasswordAsync_WithWeakPassword_ReturnsInvalidPassword()
        {
            StoreRecoveryCode();

            PasswordResetResult resetResult = await _accountService.ResetPasswordAsync(SamplePlayers.AnaEmail, StoredCode, WeakPassword);

            Assert.Equal(PasswordResetResult.InvalidPassword, resetResult);
        }

        [Fact]
        public async Task Test_RequestRecoveryCodeAsync_WithinAMinuteOfTheLastCode_ReturnsTooManyRequests()
        {
            await _accountService.RequestRecoveryCodeAsync(SamplePlayers.AnaEmail);

            RecoveryCodeRequestResult secondRequest = await _accountService.RequestRecoveryCodeAsync(SamplePlayers.AnaEmail);

            Assert.Equal(RecoveryCodeRequestResult.TooManyRequests, secondRequest);
        }

        [Fact]
        public async Task Test_RequestRecoveryCodeAsync_WithFailedEmail_ReturnsMailDeliveryFailed()
        {
            _emailSender.IsDelivering = false;

            RecoveryCodeRequestResult requestResult = await _accountService.RequestRecoveryCodeAsync(SamplePlayers.AnaEmail);

            Assert.Equal(RecoveryCodeRequestResult.MailDeliveryFailed, requestResult);
        }

        private void StoreRecoveryCode()
        {
            var storedRecovery = new PasswordRecovery
            {
                PlayerId = SamplePlayers.AnaId,
                Code = StoredCode,
                CreatedAt = _fakeTimeProvider.GetUtcNow().UtcDateTime,
            };
            _recoveryRepository.RecoveriesByPlayerId[SamplePlayers.AnaId] = storedRecovery;
        }
    }
}
