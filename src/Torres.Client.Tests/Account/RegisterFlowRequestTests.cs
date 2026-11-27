using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class RegisterFlowRequestTests
    {
        private const string ConfirmRegistrationCall = nameof(RecordingAccountConnection.ConfirmRegistrationAsync);
        private const string AbortCall = nameof(RecordingGameSessionConnection.AbortSessionChannel);
        private const string CloseSessionCall = nameof(RecordingGameSessionConnection.CloseSession);
        private const string NewUsername = "lupe_flores";
        private const string NewEmail = "lupe@torres.mx";
        private const string TypedCode = "482915";
        private const string BlankCode = "   ";

        private readonly RecordingAccountConnection _accountConnection = new RecordingAccountConnection();
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = new PlayerSession();
        private readonly RegisterFlow _registerFlow;

        public RegisterFlowRequestTests()
        {
            _accountConnection.ConfirmRegistrationReply = Task.FromResult(new RegistrationConfirmationResult { Status = RegistrationConfirmationStatus.Created, Player = SampleClients.Ana, SessionToken = SampleClients.SessionToken });
            _gameSessionConnection.OpenReply = SampleClients.CreateOpenedSessionAsync();
            _registerFlow = new RegisterFlow(_accountConnection, _gameSessionConnection, _playerSession);
        }

        [Theory]
        [InlineData(RegistrationResult.UsernameTaken, TextKeys.Register.UsernameUnavailable)]
        [InlineData(RegistrationResult.InvalidUsername, TextKeys.Register.UsernameInvalidFormat)]
        [InlineData(RegistrationResult.EmailTaken, TextKeys.Register.EmailAlreadyExists)]
        [InlineData(RegistrationResult.InvalidEmail, TextKeys.Register.EmailInvalidFormat)]
        [InlineData(RegistrationResult.InvalidPassword, TextKeys.Register.PasswordInvalidFormat)]
        [InlineData(RegistrationResult.TooManyRequests, TextKeys.Alerts.TooManyCodesBody)]
        [InlineData(RegistrationResult.MailDeliveryFailed, TextKeys.Alerts.EmailFailedBody)]
        [InlineData(RegistrationResult.DatabaseUnavailable, TextKeys.Common.ServerErrorTitle)]
        public void Test_TakeOutcome_WithARefusedCodeRequest_ShowsItsMessage(RegistrationResult requestResult, string messageKey)
        {
            RequestCodeWith(requestResult);

            RegisterOutcome? outcome = _registerFlow.TakeOutcome();

            Assert.Equal(messageKey, outcome?.MessageKey);
        }

        [Theory]
        [InlineData(RegistrationResult.UsernameTaken, (int)RegisterField.Username)]
        [InlineData(RegistrationResult.InvalidUsername, (int)RegisterField.Username)]
        [InlineData(RegistrationResult.EmailTaken, (int)RegisterField.Email)]
        [InlineData(RegistrationResult.InvalidEmail, (int)RegisterField.Email)]
        [InlineData(RegistrationResult.InvalidPassword, (int)RegisterField.Password)]
        [InlineData(RegistrationResult.TooManyRequests, (int)RegisterField.Server)]
        [InlineData(RegistrationResult.MailDeliveryFailed, (int)RegisterField.Server)]
        [InlineData(RegistrationResult.DatabaseUnavailable, (int)RegisterField.Server)]
        public void Test_TakeOutcome_WithARefusedCodeRequest_MarksItsField(RegistrationResult requestResult, int messageFieldValue)
        {
            var messageField = (RegisterField)messageFieldValue;
            RequestCodeWith(requestResult);

            RegisterOutcome? outcome = _registerFlow.TakeOutcome();

            Assert.Equal(messageField, outcome?.MessageField);
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheCodeWasSent_GoesToTheCodeStep()
        {
            RequestCodeWith(RegistrationResult.CodeSent);

            RegisterOutcome? outcome = _registerFlow.TakeOutcome();

            Assert.Equal(RegisterStep.Code, outcome?.Step);
        }

        [Fact]
        public void Test_IsRequestingCode_AfterAskingForTheCode_ReturnsTrue()
        {
            RequestCodeWith(RegistrationResult.CodeSent);

            bool isRequestingCode = _registerFlow.IsRequestingCode;

            Assert.True(isRequestingCode);
        }

        [Fact]
        public void Test_StartConfirmation_WithABlankCode_ReportsTheCodeMissing()
        {
            bool isCodeMissing = _registerFlow.StartConfirmation(BlankCode);

            Assert.True(isCodeMissing);
        }

        [Fact]
        public void Test_StartConfirmation_WithACode_AsksTheServerToConfirm()
        {
            _registerFlow.StartConfirmation(TypedCode);

            Assert.Contains(ConfirmRegistrationCall, _accountConnection.CalledMethods);
        }

        [Fact]
        public void Test_IsConfirming_AfterStartingTheConfirmation_ReturnsTrue()
        {
            _registerFlow.StartConfirmation(TypedCode);

            bool isConfirming = _registerFlow.IsConfirming;

            Assert.True(isConfirming);
        }

        [Fact]
        public void Test_TakeOutcome_WithAWrongCode_ShowsInvalidCode()
        {
            ConfirmWith(new RegistrationConfirmationResult { Status = RegistrationConfirmationStatus.InvalidCode });

            RegisterOutcome? outcome = _registerFlow.TakeOutcome();

            Assert.Equal(TextKeys.Register.InvalidCode, outcome?.MessageKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheUsernameWasTakenMeanwhile_GoesBackToTheDetails()
        {
            ConfirmWith(new RegistrationConfirmationResult { Status = RegistrationConfirmationStatus.UsernameTaken });

            RegisterOutcome? outcome = _registerFlow.TakeOutcome();

            Assert.Equal(RegisterStep.Details, outcome?.Step);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheEmailWasTakenMeanwhile_ShowsEmailAlreadyExists()
        {
            ConfirmWith(new RegistrationConfirmationResult { Status = RegistrationConfirmationStatus.EmailTaken });

            RegisterOutcome? outcome = _registerFlow.TakeOutcome();

            Assert.Equal(TextKeys.Register.EmailAlreadyExists, outcome?.MessageKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheConfirmationFindsTheDatabaseUnavailable_ShowsTheServerError()
        {
            ConfirmWith(new RegistrationConfirmationResult { Status = RegistrationConfirmationStatus.DatabaseUnavailable });

            RegisterOutcome? outcome = _registerFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.MessageKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheConfirmationFails_ShowsTheServerError()
        {
            _accountConnection.ConfirmRegistrationReply = SampleClients.CreateFailedCallAsync<RegistrationConfirmationResult>();
            _registerFlow.StartConfirmation(TypedCode);

            RegisterOutcome? outcome = _registerFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.MessageKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheAccountComesWithoutToken_ShowsTheServerError()
        {
            ConfirmWith(new RegistrationConfirmationResult { Status = RegistrationConfirmationStatus.Created, Player = SampleClients.Ana });

            RegisterOutcome? outcome = _registerFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.MessageKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheSessionIsRefused_AbortsTheChannel()
        {
            _gameSessionConnection.OpenReply = Task.FromResult(new OpenSessionResult { Status = OpenSessionStatus.InvalidSessionToken });
            _registerFlow.StartConfirmation(TypedCode);

            _registerFlow.TakeOutcome();

            Assert.Contains(AbortCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_RegisteringAfterPlayingAsGuest_ClosesTheGuestSession()
        {
            _playerSession.StartAsGuest(SampleClients.GuestAlias);
            _registerFlow.StartConfirmation(TypedCode);

            _registerFlow.TakeOutcome();

            Assert.Contains(CloseSessionCall, _gameSessionConnection.CalledMethods);
        }

        private void RequestCodeWith(RegistrationResult requestResult)
        {
            _accountConnection.RequestRegistrationCodeReply = Task.FromResult(requestResult);
            _registerFlow.StartCodeRequest(NewUsername, NewEmail, SampleClients.Password);
        }

        private void ConfirmWith(RegistrationConfirmationResult confirmation)
        {
            _accountConnection.ConfirmRegistrationReply = Task.FromResult(confirmation);
            _registerFlow.StartConfirmation(TypedCode);
        }
    }
}
