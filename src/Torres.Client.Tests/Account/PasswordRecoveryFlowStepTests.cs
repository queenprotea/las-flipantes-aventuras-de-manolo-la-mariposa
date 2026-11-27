using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class PasswordRecoveryFlowStepTests
    {
        private const string TypedCode = "482915";
        private const string BlankCode = "   ";
        private const string NewPassword = "Nueva#2026";

        private readonly RecordingAccountConnection _accountConnection = new RecordingAccountConnection();
        private readonly PasswordRecoveryFlow _recoveryFlow;

        public PasswordRecoveryFlowStepTests()
        {
            _recoveryFlow = new PasswordRecoveryFlow(_accountConnection);
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheCodeWasSent_GoesToTheCodeStep()
        {
            RequestCodeWith(RecoveryCodeRequestResult.CodeSent);

            PasswordRecoveryOutcome? outcome = _recoveryFlow.TakeOutcome();

            Assert.Equal(PasswordRecoveryStep.Code, outcome?.Step);
        }

        [Theory]
        [InlineData(RecoveryCodeRequestResult.InvalidEmail, TextKeys.PasswordRecovery.InvalidEmail)]
        [InlineData(RecoveryCodeRequestResult.TooManyRequests, TextKeys.Alerts.TooManyCodesBody)]
        [InlineData(RecoveryCodeRequestResult.MailDeliveryFailed, TextKeys.Alerts.EmailFailedBody)]
        public void Test_TakeOutcome_WithARefusedCodeRequest_ShowsItsMessage(RecoveryCodeRequestResult requestResult, string messageKey)
        {
            RequestCodeWith(requestResult);

            PasswordRecoveryOutcome? outcome = _recoveryFlow.TakeOutcome();

            Assert.Equal(messageKey, outcome?.MessageKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheCodeRequestFindsTheDatabaseUnavailable_EndsAsServerUnreachable()
        {
            RequestCodeWith(RecoveryCodeRequestResult.DatabaseUnavailable);

            PasswordRecoveryOutcome? outcome = _recoveryFlow.TakeOutcome();

            Assert.Equal(PasswordRecoveryEnding.ServerUnreachable, outcome?.Ending);
        }

        [Fact]
        public void Test_IsSendingCode_AfterAskingForTheCode_ReturnsTrue()
        {
            RequestCodeWith(RecoveryCodeRequestResult.CodeSent);

            bool isSendingCode = _recoveryFlow.IsSendingCode;

            Assert.True(isSendingCode);
        }

        [Fact]
        public void Test_StartVerification_WithABlankCode_ReturnsFalse()
        {
            bool isTyped = _recoveryFlow.StartVerification(BlankCode);

            Assert.False(isTyped);
        }

        [Fact]
        public void Test_TakeOutcome_WithAValidCode_GoesToThePasswordStep()
        {
            VerifyWith(RecoveryCodeVerificationResult.Valid);

            PasswordRecoveryOutcome? outcome = _recoveryFlow.TakeOutcome();

            Assert.Equal(PasswordRecoveryStep.Password, outcome?.Step);
        }

        [Fact]
        public void Test_TakeOutcome_WithAWrongCode_ShowsInvalidCode()
        {
            VerifyWith(RecoveryCodeVerificationResult.InvalidCode);

            PasswordRecoveryOutcome? outcome = _recoveryFlow.TakeOutcome();

            Assert.Equal(TextKeys.PasswordRecovery.InvalidCode, outcome?.MessageKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheVerificationFails_EndsAsServerUnreachable()
        {
            _accountConnection.VerifyRecoveryCodeReply = SampleClients.CreateFailedCallAsync<RecoveryCodeVerificationResult>();
            _recoveryFlow.StartVerification(TypedCode);

            PasswordRecoveryOutcome? outcome = _recoveryFlow.TakeOutcome();

            Assert.Equal(PasswordRecoveryEnding.ServerUnreachable, outcome?.Ending);
        }

        [Fact]
        public void Test_IsVerifying_AfterStartingTheVerification_ReturnsTrue()
        {
            VerifyWith(RecoveryCodeVerificationResult.Valid);

            bool isVerifying = _recoveryFlow.IsVerifying;

            Assert.True(isVerifying);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheCodeExpiredBeforeTheReset_GoesBackToTheCodeStep()
        {
            ResetWith(PasswordResetResult.InvalidCode);

            PasswordRecoveryOutcome? outcome = _recoveryFlow.TakeOutcome();

            Assert.Equal(PasswordRecoveryStep.Code, outcome?.Step);
        }

        [Fact]
        public void Test_TakeOutcome_WithAnInvalidNewPassword_ShowsTheFormatError()
        {
            ResetWith(PasswordResetResult.InvalidPassword);

            PasswordRecoveryOutcome? outcome = _recoveryFlow.TakeOutcome();

            Assert.Equal(TextKeys.PasswordRecovery.PasswordInvalidFormat, outcome?.MessageKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheResetFindsTheDatabaseUnavailable_EndsAsServerUnreachable()
        {
            ResetWith(PasswordResetResult.DatabaseUnavailable);

            PasswordRecoveryOutcome? outcome = _recoveryFlow.TakeOutcome();

            Assert.Equal(PasswordRecoveryEnding.ServerUnreachable, outcome?.Ending);
        }

        [Fact]
        public void Test_IsSaving_AfterStartingTheReset_ReturnsTrue()
        {
            ResetWith(PasswordResetResult.Reset);

            bool isSaving = _recoveryFlow.IsSaving;

            Assert.True(isSaving);
        }

        private void RequestCodeWith(RecoveryCodeRequestResult requestResult)
        {
            _accountConnection.RequestRecoveryCodeReply = Task.FromResult(requestResult);
            _recoveryFlow.StartCodeRequest(SampleClients.AnaEmail);
        }

        private void VerifyWith(RecoveryCodeVerificationResult verification)
        {
            _accountConnection.VerifyRecoveryCodeReply = Task.FromResult(verification);
            _recoveryFlow.StartVerification(TypedCode);
        }

        private void ResetWith(PasswordResetResult resetResult)
        {
            _accountConnection.ResetPasswordReply = Task.FromResult(resetResult);
            _recoveryFlow.StartReset(NewPassword, NewPassword);
        }
    }
}
