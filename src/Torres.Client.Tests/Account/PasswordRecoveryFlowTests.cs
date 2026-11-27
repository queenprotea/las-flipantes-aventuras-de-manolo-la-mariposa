using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class PasswordRecoveryFlowTests
    {
        private const string NewPassword = "Nueva#2027";
        private const string OtherPassword = "Otra#2027x";
        private const string EmailWithoutAtSign = "ana-sin-arroba";

        private readonly RecordingAccountConnection _accountConnection = new RecordingAccountConnection();
        private readonly PasswordRecoveryFlow _passwordRecoveryFlow;

        public PasswordRecoveryFlowTests()
        {
            _passwordRecoveryFlow = new PasswordRecoveryFlow(_accountConnection);
        }

        [Fact]
        public void Test_TakeOutcome_AfterResettingThePassword_EndsAsPasswordReset()
        {
            _accountConnection.ResetPasswordReply = Task.FromResult(PasswordResetResult.Reset);
            _passwordRecoveryFlow.StartReset(NewPassword, NewPassword);

            PasswordRecoveryOutcome? outcome = _passwordRecoveryFlow.TakeOutcome();

            Assert.Equal(PasswordRecoveryEnding.PasswordReset, outcome?.Ending);
        }

        [Fact]
        public void Test_StartCodeRequest_WithoutAtSign_ReturnsFalse()
        {
            bool isValid = _passwordRecoveryFlow.StartCodeRequest(EmailWithoutAtSign);

            Assert.False(isValid);
        }

        [Fact]
        public void Test_StartReset_WithDifferentPasswords_ReturnsFalse()
        {
            bool isRepeated = _passwordRecoveryFlow.StartReset(NewPassword, OtherPassword);

            Assert.False(isRepeated);
        }

        [Fact]
        public void Test_StartReset_WithDifferentPasswords_DoesNotCallTheServer()
        {
            _passwordRecoveryFlow.StartReset(NewPassword, OtherPassword);

            Assert.Empty(_accountConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_WithServerUnreachable_EndsAsServerUnreachable()
        {
            _accountConnection.RequestRecoveryCodeReply = SampleClients.CreateFailedCallAsync<RecoveryCodeRequestResult>();
            _passwordRecoveryFlow.StartCodeRequest(SampleClients.AnaEmail);

            PasswordRecoveryOutcome? outcome = _passwordRecoveryFlow.TakeOutcome();

            Assert.Equal(PasswordRecoveryEnding.ServerUnreachable, outcome?.Ending);
        }

        [Fact]
        public void Test_FindScreenToCancel_FromTheRecovery_ReturnsLogin()
        {
            ScreenId nextScreen = PasswordRecoveryFlow.FindScreenToCancel();

            Assert.Equal(ScreenId.Login, nextScreen);
        }
    }
}
