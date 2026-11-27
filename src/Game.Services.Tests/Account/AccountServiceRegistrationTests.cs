using System;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class AccountServiceRegistrationTests
    {
        private const string NewUsername = "lupe_flores";
        private const string NewEmail = "lupe@torres.mx";
        private const string HeldCode = "482915";
        private const string WrongCode = "000000";
        private const string BadUsername = "lu";
        private const string BadEmail = "lupe-sin-arroba";
        private const string WeakPassword = "abc";
        private const int CodeLifetimeMinutes = 10;

        private static readonly byte[] _signingKey = new byte[SessionTokenIssuer.ShortestSigningKeyBytes];
        private static readonly TimeSpan _codeLifetime = TimeSpan.FromMinutes(CodeLifetimeMinutes);

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly RecordingPlayerRepository _playerRepository = new RecordingPlayerRepository();
        private readonly RecordingEmailSender _emailSender = new RecordingEmailSender();
        private readonly PendingRegistrationRegistry _pendingRegistrationRegistry;
        private readonly AccountService _accountService;

        public AccountServiceRegistrationTests()
        {
            var tokenIssuer = new SessionTokenIssuer(_signingKey, _fakeTimeProvider);
            _pendingRegistrationRegistry = new PendingRegistrationRegistry(_fakeTimeProvider);
            var verificationCodeLifetime = new VerificationCodeLifetime();
            verificationCodeLifetime.Start(CodeLifetimeMinutes);
            var accountRegistration = new AccountRegistration(_playerRepository, _emailSender, _pendingRegistrationRegistry, tokenIssuer, _fakeTimeProvider, verificationCodeLifetime);
            var accessRecovery = new AccessRecovery(_playerRepository, new RecordingPasswordRecoveryRepository(), _emailSender, _fakeTimeProvider, verificationCodeLifetime);
            _accountService = new AccountService(_playerRepository, new RecordingSanctionRepository(), tokenIssuer, new GuestIdIssuer(), accountRegistration, accessRecovery);
        }

        [Fact]
        public async Task Test_RequestRegistrationCodeAsync_WithFreeAccount_ReturnsCodeSent()
        {
            RegistrationResult requestResult = await _accountService.RequestRegistrationCodeAsync(NewUsername, NewEmail, SamplePlayers.Password);

            Assert.Equal(RegistrationResult.CodeSent, requestResult);
        }

        [Fact]
        public async Task Test_RequestRegistrationCodeAsync_WithFreeAccount_EmailsTheCodeToTheNewAddress()
        {
            await _accountService.RequestRegistrationCodeAsync(NewUsername, NewEmail, SamplePlayers.Password);

            Assert.Equal(new string[] { NewEmail }, _emailSender.SentMessages.Select(sentMessage => sentMessage.RecipientAddress));
        }

        [Fact]
        public async Task Test_ConfirmRegistrationAsync_WithHeldCode_ReturnsCreated()
        {
            HoldRegistration(NewUsername, NewEmail);

            RegistrationConfirmationResult confirmation = await _accountService.ConfirmRegistrationAsync(NewEmail, HeldCode);

            Assert.Equal(RegistrationConfirmationStatus.Created, confirmation.Status);
        }

        [Fact]
        public async Task Test_ConfirmRegistrationAsync_WithHeldCode_StoresTheNewAccount()
        {
            HoldRegistration(NewUsername, NewEmail);

            await _accountService.ConfirmRegistrationAsync(NewEmail, HeldCode);

            Assert.NotNull(_playerRepository.FindByUsername(NewUsername));
        }

        [Fact]
        public async Task Test_RequestRegistrationCodeAsync_WithTakenUsername_ReturnsUsernameTaken()
        {
            RegistrationResult requestResult = await _accountService.RequestRegistrationCodeAsync(SamplePlayers.AnaUsername, NewEmail, SamplePlayers.Password);

            Assert.Equal(RegistrationResult.UsernameTaken, requestResult);
        }

        [Fact]
        public async Task Test_RequestRegistrationCodeAsync_WithTakenEmail_ReturnsEmailTaken()
        {
            RegistrationResult requestResult = await _accountService.RequestRegistrationCodeAsync(NewUsername, SamplePlayers.AnaEmail, SamplePlayers.Password);

            Assert.Equal(RegistrationResult.EmailTaken, requestResult);
        }

        [Fact]
        public async Task Test_RequestRegistrationCodeAsync_WithBadUsername_ReturnsInvalidUsername()
        {
            RegistrationResult requestResult = await _accountService.RequestRegistrationCodeAsync(BadUsername, NewEmail, SamplePlayers.Password);

            Assert.Equal(RegistrationResult.InvalidUsername, requestResult);
        }

        [Fact]
        public async Task Test_RequestRegistrationCodeAsync_WithBadEmail_ReturnsInvalidEmail()
        {
            RegistrationResult requestResult = await _accountService.RequestRegistrationCodeAsync(NewUsername, BadEmail, SamplePlayers.Password);

            Assert.Equal(RegistrationResult.InvalidEmail, requestResult);
        }

        [Fact]
        public async Task Test_RequestRegistrationCodeAsync_WithWeakPassword_ReturnsInvalidPassword()
        {
            RegistrationResult requestResult = await _accountService.RequestRegistrationCodeAsync(NewUsername, NewEmail, WeakPassword);

            Assert.Equal(RegistrationResult.InvalidPassword, requestResult);
        }

        [Fact]
        public async Task Test_RequestRegistrationCodeAsync_WithinAMinuteOfTheLastCode_ReturnsTooManyRequests()
        {
            await _accountService.RequestRegistrationCodeAsync(NewUsername, NewEmail, SamplePlayers.Password);

            RegistrationResult secondRequest = await _accountService.RequestRegistrationCodeAsync(NewUsername, NewEmail, SamplePlayers.Password);

            Assert.Equal(RegistrationResult.TooManyRequests, secondRequest);
        }

        [Fact]
        public async Task Test_ConfirmRegistrationAsync_WithWrongCode_ReturnsInvalidCode()
        {
            HoldRegistration(NewUsername, NewEmail);

            RegistrationConfirmationResult confirmation = await _accountService.ConfirmRegistrationAsync(NewEmail, WrongCode);

            Assert.Equal(RegistrationConfirmationStatus.InvalidCode, confirmation.Status);
        }

        [Fact]
        public async Task Test_ConfirmRegistrationAsync_WithExpiredCode_ReturnsInvalidCode()
        {
            HoldRegistration(NewUsername, NewEmail);
            _fakeTimeProvider.Advance(_codeLifetime);

            RegistrationConfirmationResult confirmation = await _accountService.ConfirmRegistrationAsync(NewEmail, HeldCode);

            Assert.Equal(RegistrationConfirmationStatus.InvalidCode, confirmation.Status);
        }

        [Fact]
        public async Task Test_ConfirmRegistrationAsync_AfterTheUsernameWasTakenMeanwhile_ReturnsUsernameTaken()
        {
            HoldRegistration(SamplePlayers.AnaUsername, NewEmail);

            RegistrationConfirmationResult confirmation = await _accountService.ConfirmRegistrationAsync(NewEmail, HeldCode);

            Assert.Equal(RegistrationConfirmationStatus.UsernameTaken, confirmation.Status);
        }

        [Fact]
        public async Task Test_ConfirmRegistrationAsync_AfterTheEmailWasTakenMeanwhile_ReturnsEmailTaken()
        {
            HoldRegistration(NewUsername, SamplePlayers.AnaEmail);

            RegistrationConfirmationResult confirmation = await _accountService.ConfirmRegistrationAsync(SamplePlayers.AnaEmail, HeldCode);

            Assert.Equal(RegistrationConfirmationStatus.EmailTaken, confirmation.Status);
        }

        [Fact]
        public async Task Test_RequestRegistrationCodeAsync_WithFailedEmail_ReturnsMailDeliveryFailed()
        {
            _emailSender.IsDelivering = false;

            RegistrationResult requestResult = await _accountService.RequestRegistrationCodeAsync(NewUsername, NewEmail, SamplePlayers.Password);

            Assert.Equal(RegistrationResult.MailDeliveryFailed, requestResult);
        }

        private void HoldRegistration(string username, string email)
        {
            DateTimeOffset expiresAt = _fakeTimeProvider.GetUtcNow() + _codeLifetime;
            _pendingRegistrationRegistry.Hold(new PendingRegistration(username, email, SamplePlayers.PasswordHash, HeldCode, expiresAt));
        }
    }
}
