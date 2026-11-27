using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class RegisterFlowTests
    {
        private const string NewUsername = "lupe_flores";
        private const string NewEmail = "lupe@torres.mx";
        private const string TypedCode = "482915";

        private static readonly RegisterField[] _allFields = new RegisterField[] { RegisterField.Username, RegisterField.Email, RegisterField.Password };

        private readonly RecordingAccountConnection _accountConnection = new RecordingAccountConnection();
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly RegisterFlow _registerFlow;

        public RegisterFlowTests()
        {
            _registerFlow = new RegisterFlow(_accountConnection, _gameSessionConnection, new PlayerSession());
            _accountConnection.RequestRegistrationCodeReply = Task.FromResult(RegistrationResult.CodeSent);
            _accountConnection.ConfirmRegistrationReply = Task.FromResult(new RegistrationConfirmationResult { Status = RegistrationConfirmationStatus.Created, Player = SampleClients.Ana, SessionToken = SampleClients.SessionToken });
            _gameSessionConnection.OpenReply = SampleClients.CreateOpenedSessionAsync();
        }

        [Fact]
        public void Test_TakeOutcome_AfterConfirmingTheCode_GoesToTheMainMenu()
        {
            _registerFlow.StartCodeRequest(NewUsername, NewEmail, SampleClients.Password);
            _registerFlow.TakeOutcome();
            _registerFlow.StartConfirmation(TypedCode);

            RegisterOutcome? outcome = _registerFlow.TakeOutcome();

            Assert.Equal(ScreenId.MainMenu, outcome?.NextScreen);
        }

        [Fact]
        public void Test_StartCodeRequest_WithEmptyFields_ListsTheMissingFields()
        {
            IReadOnlyList<RegisterField> missingFields = _registerFlow.StartCodeRequest(string.Empty, string.Empty, string.Empty);

            Assert.Equal(_allFields, missingFields);
        }

        [Fact]
        public void Test_TakeOutcome_WithServerUnreachable_ShowsTheServerError()
        {
            _accountConnection.RequestRegistrationCodeReply = SampleClients.CreateFailedCallAsync<RegistrationResult>();
            _registerFlow.StartCodeRequest(NewUsername, NewEmail, SampleClients.Password);

            RegisterOutcome? outcome = _registerFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.MessageKey);
        }

        [Fact]
        public void Test_FindScreenToCancel_FromTheRegistration_ReturnsLogin()
        {
            ScreenId nextScreen = RegisterFlow.FindScreenToCancel();

            Assert.Equal(ScreenId.Login, nextScreen);
        }
    }
}
