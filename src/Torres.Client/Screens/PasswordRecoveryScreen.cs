using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class PasswordRecoveryScreen : Screen
    {
        private readonly ChannelFactory<IAccountService> _accountChannelFactory;
        private readonly VerticalStackPanel _emailStep = CreateCard(Sizes.RecoveryCardWidth);
        private readonly VerticalStackPanel _codeStep = CreateCard(Sizes.RecoveryCardWidth);
        private readonly VerticalStackPanel _passwordStep = CreateCard(Sizes.RecoveryCardWidth);
        private readonly LabeledTextBox _emailField = new LabeledTextBox(TextKeys.PasswordRecovery.EmailLabel, false, AccountLimits.LongestEmail);
        private readonly LabeledTextBox _codeField = new LabeledTextBox(TextKeys.PasswordRecovery.CodeLabel, false, AccountLimits.VerificationCodeLength);
        private readonly LabeledTextBox _newPasswordField = new LabeledTextBox(TextKeys.PasswordRecovery.NewPasswordLabel, true, AccountLimits.LongestPassword);
        private readonly LabeledTextBox _repeatPasswordField = new LabeledTextBox(TextKeys.PasswordRecovery.RepeatPasswordLabel, true, AccountLimits.LongestPassword);
        private readonly LocalizedLabel _emailError = CreateErrorMessage(TextKeys.PasswordRecovery.InvalidEmail);
        private readonly LocalizedLabel _codeError = CreateErrorMessage(TextKeys.PasswordRecovery.InvalidCode);
        private readonly LocalizedLabel _passwordError = CreateErrorMessage(TextKeys.PasswordRecovery.PasswordsDoNotMatch);
        private readonly LocalizedButton _sendCodeButton = CreatePrimaryButton(TextKeys.PasswordRecovery.SendCodeButton);
        private readonly LocalizedButton _verifyButton = CreatePrimaryButton(TextKeys.PasswordRecovery.VerifyButton);
        private readonly LocalizedButton _saveButton = CreatePrimaryButton(TextKeys.PasswordRecovery.SaveButton);

        private IAccountService? _accountChannel;
        private Task<RecoveryCodeRequestResult>? _codeRequest;
        private Task<RecoveryCodeVerificationResult>? _codeVerification;
        private Task<PasswordResetResult>? _passwordReset;
        private string _requestedEmail = string.Empty;
        private string _verifiedCode = string.Empty;

        internal PasswordRecoveryScreen(ChannelFactory<IAccountService> accountChannelFactory)
            : base(TextKeys.PasswordRecovery.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(accountChannelFactory);

            _accountChannelFactory = accountChannelFactory;
        }

        internal bool WasServerUnreachable { get; private set; }

        internal bool WasPasswordReset { get; private set; }

        internal void ClearRequests()
        {
            WasServerUnreachable = false;
            WasPasswordReset = false;
        }

        internal override void Open()
        {
            _codeField.Box.Text = string.Empty;
            _newPasswordField.Box.Text = string.Empty;
            _repeatPasswordField.Box.Text = string.Empty;
            _emailError.Visible = false;
            ShowStep(_emailStep);
        }

        internal override void Update(GameTime gameTime)
        {
            FollowCodeRequest();
            FollowCodeVerification();
            FollowPasswordReset();
        }

        protected override Widget Build()
        {
            var screenLayout = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
            };
            screenLayout.Widgets.Add(BuildEmailStep());
            screenLayout.Widgets.Add(BuildCodeStep());
            screenLayout.Widgets.Add(BuildPasswordStep());

            ShowStep(_emailStep);

            return screenLayout;
        }

        private VerticalStackPanel BuildEmailStep()
        {
            _sendCodeButton.Click += SendCodeButtonOnClick;
            _sendCodeButton.HorizontalAlignment = HorizontalAlignment.Left;
            VerticalStackPanel stepActions = CreateWrappedButtonRow();
            stepActions.Widgets.Add(_sendCodeButton);
            stepActions.Widgets.Add(BackToLoginButton());

            _emailStep.HorizontalAlignment = HorizontalAlignment.Left;
            _emailStep.Widgets.Add(CreateCardHeader(TextKeys.PasswordRecovery.Step1Title, TextKeys.PasswordRecovery.Step1Hint));
            _emailStep.Widgets.Add(BuildField(_emailField, _emailError));
            _emailStep.Widgets.Add(stepActions);

            return _emailStep;
        }

        private VerticalStackPanel BuildCodeStep()
        {
            LocalizedButton newCodeButton = CreateSecondaryButton(TextKeys.Common.BackButton);
            _verifyButton.Click += VerifyButtonOnClick;
            newCodeButton.Click += NewCodeButtonOnClick;
            HorizontalStackPanel stepActions = CreateButtonRow();
            stepActions.Widgets.Add(_verifyButton);
            stepActions.Widgets.Add(newCodeButton);

            _codeStep.HorizontalAlignment = HorizontalAlignment.Left;
            _codeStep.Visible = false;
            _codeStep.Widgets.Add(CreateCardHeader(TextKeys.PasswordRecovery.Step2Title, TextKeys.PasswordRecovery.Step2Hint));
            _codeStep.Widgets.Add(BuildField(_codeField, _codeError));
            _codeStep.Widgets.Add(stepActions);

            return _codeStep;
        }

        private VerticalStackPanel BuildPasswordStep()
        {
            _saveButton.Click += SaveButtonOnClick;
            _saveButton.HorizontalAlignment = HorizontalAlignment.Left;
            VerticalStackPanel stepActions = CreateWrappedButtonRow();
            stepActions.Widgets.Add(_saveButton);
            stepActions.Widgets.Add(BackToLoginButton());

            _passwordStep.HorizontalAlignment = HorizontalAlignment.Left;
            _passwordStep.Visible = false;
            _passwordStep.Widgets.Add(CreateCardHeader(TextKeys.PasswordRecovery.Step3Title, TextKeys.PasswordRecovery.Step3Hint));
            _passwordStep.Widgets.Add(_newPasswordField);
            _passwordStep.Widgets.Add(BuildField(_repeatPasswordField, _passwordError));
            _passwordStep.Widgets.Add(stepActions);

            return _passwordStep;
        }

        private static VerticalStackPanel BuildField(LabeledTextBox textBox, LocalizedLabel messageLabel)
        {
            var fieldGroup = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            fieldGroup.Widgets.Add(textBox);
            fieldGroup.Widgets.Add(messageLabel);

            return fieldGroup;
        }

        private LocalizedButton BackToLoginButton()
        {
            LocalizedButton backButton = CreateSecondaryButton(TextKeys.PasswordRecovery.BackToLoginButton);
            backButton.HorizontalAlignment = HorizontalAlignment.Left;
            backButton.Click += BackButtonOnClick;

            return backButton;
        }

        private void ShowStep(VerticalStackPanel visibleStep)
        {
            _emailStep.Visible = visibleStep == _emailStep;
            _codeStep.Visible = visibleStep == _codeStep;
            _passwordStep.Visible = visibleStep == _passwordStep;
        }

        private void SendCodeButtonOnClick(object sender, MyraEventArgs e)
        {
            string typedEmail = _emailField.Value.Trim();
            bool isValid = typedEmail.Contains('@');
            ShowMessage(_emailError, TextKeys.PasswordRecovery.InvalidEmail, !isValid);
            if (isValid)
            {
                _sendCodeButton.Enabled = false;
                _requestedEmail = typedEmail;
                _accountChannel = _accountChannelFactory.CreateChannel();
                _codeRequest = _accountChannel.RequestRecoveryCodeAsync(_requestedEmail);
            }
        }

        private void VerifyButtonOnClick(object sender, MyraEventArgs e)
        {
            string typedCode = _codeField.Value.Trim();
            bool isTyped = typedCode.Length > 0;
            ShowMessage(_codeError, TextKeys.PasswordRecovery.InvalidCode, !isTyped);
            if (isTyped)
            {
                _verifyButton.Enabled = false;
                _verifiedCode = typedCode;
                _accountChannel = _accountChannelFactory.CreateChannel();
                _codeVerification = _accountChannel.VerifyRecoveryCodeAsync(_requestedEmail, _verifiedCode);
            }
        }

        private void SaveButtonOnClick(object sender, MyraEventArgs e)
        {
            bool isRepeated = string.Equals(_newPasswordField.Value, _repeatPasswordField.Value, StringComparison.Ordinal);
            ShowMessage(_passwordError, TextKeys.PasswordRecovery.PasswordsDoNotMatch, !isRepeated);
            if (isRepeated)
            {
                _saveButton.Enabled = false;
                _accountChannel = _accountChannelFactory.CreateChannel();
                _passwordReset = _accountChannel.ResetPasswordAsync(_requestedEmail, _verifiedCode, _newPasswordField.Value);
            }
        }

        private void FollowCodeRequest()
        {
            if ((_codeRequest is null) || !_codeRequest.IsCompleted)
            {
                return;
            }

            Task<RecoveryCodeRequestResult> completedRequest = _codeRequest;
            _codeRequest = null;
            _sendCodeButton.Enabled = true;
            AbortChannel();
            RecoveryCodeRequestResult requestResult = completedRequest.IsCompletedSuccessfully
                ? completedRequest.Result
                : RecoveryCodeRequestResult.DatabaseUnavailable;
            ShowCodeRequestResult(requestResult);
        }

        private void ShowCodeRequestResult(RecoveryCodeRequestResult requestResult)
        {
            switch (requestResult)
            {
                case RecoveryCodeRequestResult.CodeSent:
                    _codeField.Box.Text = string.Empty;
                    _codeError.Visible = false;
                    ShowStep(_codeStep);
                    break;

                case RecoveryCodeRequestResult.InvalidEmail:
                    ShowMessage(_emailError, TextKeys.PasswordRecovery.InvalidEmail, true);
                    break;

                case RecoveryCodeRequestResult.TooManyRequests:
                    ShowMessage(_emailError, TextKeys.Alerts.TooManyCodesBody, true);
                    break;

                case RecoveryCodeRequestResult.MailDeliveryFailed:
                    ShowMessage(_emailError, TextKeys.Alerts.EmailFailedBody, true);
                    break;

                case RecoveryCodeRequestResult.DatabaseUnavailable:
                    WasServerUnreachable = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(requestResult), requestResult, null);
            }
        }

        private void FollowCodeVerification()
        {
            if ((_codeVerification is null) || !_codeVerification.IsCompleted)
            {
                return;
            }

            Task<RecoveryCodeVerificationResult> completedVerification = _codeVerification;
            _codeVerification = null;
            _verifyButton.Enabled = true;
            AbortChannel();
            RecoveryCodeVerificationResult verification = completedVerification.IsCompletedSuccessfully
                ? completedVerification.Result
                : RecoveryCodeVerificationResult.DatabaseUnavailable;
            ShowVerificationResult(verification);
        }

        private void ShowVerificationResult(RecoveryCodeVerificationResult verification)
        {
            switch (verification)
            {
                case RecoveryCodeVerificationResult.Valid:
                    _passwordError.Visible = false;
                    ShowStep(_passwordStep);
                    break;

                case RecoveryCodeVerificationResult.InvalidCode:
                    ShowMessage(_codeError, TextKeys.PasswordRecovery.InvalidCode, true);
                    break;

                case RecoveryCodeVerificationResult.DatabaseUnavailable:
                    WasServerUnreachable = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(verification), verification, null);
            }
        }

        private void FollowPasswordReset()
        {
            if ((_passwordReset is null) || !_passwordReset.IsCompleted)
            {
                return;
            }

            Task<PasswordResetResult> completedReset = _passwordReset;
            _passwordReset = null;
            _saveButton.Enabled = true;
            AbortChannel();
            PasswordResetResult resetResult = completedReset.IsCompletedSuccessfully
                ? completedReset.Result
                : PasswordResetResult.DatabaseUnavailable;
            ShowResetResult(resetResult);
        }

        private void ShowResetResult(PasswordResetResult resetResult)
        {
            switch (resetResult)
            {
                case PasswordResetResult.Reset:
                    WasPasswordReset = true;
                    break;

                case PasswordResetResult.InvalidCode:
                    ShowStep(_codeStep);
                    ShowMessage(_codeError, TextKeys.PasswordRecovery.InvalidCode, true);
                    break;

                case PasswordResetResult.InvalidPassword:
                    ShowMessage(_passwordError, TextKeys.PasswordRecovery.PasswordInvalidFormat, true);
                    break;

                case PasswordResetResult.DatabaseUnavailable:
                    WasServerUnreachable = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(resetResult), resetResult, null);
            }
        }

        private static void ShowMessage(LocalizedLabel messageLabel, string textKey, bool isVisible)
        {
            messageLabel.TextKey = textKey;
            messageLabel.Visible = isVisible;
        }

        private void AbortChannel()
        {
            if (_accountChannel is ICommunicationObject usedChannel)
            {
                usedChannel.Abort();
            }

            _accountChannel = null;
        }

        private void NewCodeButtonOnClick(object sender, MyraEventArgs e)
        {
            ShowStep(_emailStep);
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Login;
        }
    }
}
