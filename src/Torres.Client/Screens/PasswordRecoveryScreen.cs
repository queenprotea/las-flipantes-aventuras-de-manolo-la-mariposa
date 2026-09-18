using Myra.Graphics2D.UI;

using Torres.Client.Localization;

namespace Torres.Client.Screens
{
    internal sealed class PasswordRecoveryScreen : Screen
    {
        private VerticalStackPanel? _emailStep;
        private VerticalStackPanel? _codeStep;
        private VerticalStackPanel? _passwordStep;
        private LabeledTextBox? _emailField;
        private LabeledTextBox? _codeField;
        private LocalizedLabel? _emailError;
        private LocalizedLabel? _codeError;

        internal PasswordRecoveryScreen()
            : base(TextKeys.PasswordRecovery.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            var panel = Card();
            panel.Widgets.Add(BuildEmailStep());
            panel.Widgets.Add(BuildCodeStep());
            panel.Widgets.Add(BuildPasswordStep());

            LocalizedButton backButton = SecondaryButton(TextKeys.PasswordRecovery.BackToLoginButton);
            backButton.Click += OnBackClick;
            panel.Widgets.Add(backButton);

            ShowStep(_emailStep!);
            return panel;
        }

        private VerticalStackPanel BuildEmailStep()
        {
            _emailField = new LabeledTextBox(TextKeys.PasswordRecovery.EmailLabel, false);
            _emailError = Error(TextKeys.PasswordRecovery.InvalidEmail);
            LocalizedButton sendCodeButton = PrimaryButton(TextKeys.PasswordRecovery.SendCodeButton);
            sendCodeButton.Click += OnSendCodeClick;

            _emailStep = new VerticalStackPanel { Spacing = 8 };
            _emailStep.Widgets.Add(Title(TextKeys.PasswordRecovery.Step1Title));
            _emailStep.Widgets.Add(Hint(TextKeys.PasswordRecovery.Step1Hint));
            _emailStep.Widgets.Add(_emailField);
            _emailStep.Widgets.Add(_emailError);
            _emailStep.Widgets.Add(sendCodeButton);
            return _emailStep;
        }

        private VerticalStackPanel BuildCodeStep()
        {
            _codeField = new LabeledTextBox(TextKeys.PasswordRecovery.CodeLabel, false);
            _codeError = Error(TextKeys.PasswordRecovery.InvalidCode);
            LocalizedButton verifyButton = PrimaryButton(TextKeys.PasswordRecovery.VerifyButton);
            verifyButton.Click += OnVerifyClick;

            _codeStep = new VerticalStackPanel { Spacing = 8, Visible = false };
            _codeStep.Widgets.Add(Title(TextKeys.PasswordRecovery.Step2Title));
            _codeStep.Widgets.Add(Hint(TextKeys.PasswordRecovery.Step2Hint));
            _codeStep.Widgets.Add(_codeField);
            _codeStep.Widgets.Add(_codeError);
            _codeStep.Widgets.Add(verifyButton);
            return _codeStep;
        }

        private VerticalStackPanel BuildPasswordStep()
        {
            _passwordStep = new VerticalStackPanel { Spacing = 8, Visible = false };
            _passwordStep.Widgets.Add(Title(TextKeys.PasswordRecovery.Step3Title));
            _passwordStep.Widgets.Add(Hint(TextKeys.PasswordRecovery.Step3Hint));
            _passwordStep.Widgets.Add(new LabeledTextBox(TextKeys.PasswordRecovery.NewPasswordLabel, true));
            _passwordStep.Widgets.Add(new LabeledTextBox(TextKeys.PasswordRecovery.RepeatPasswordLabel, true));
            _passwordStep.Widgets.Add(PrimaryButton(TextKeys.PasswordRecovery.SaveButton));
            return _passwordStep;
        }

        private void ShowStep(VerticalStackPanel step)
        {
            _emailStep!.Visible = step == _emailStep;
            _codeStep!.Visible = step == _codeStep;
            _passwordStep!.Visible = step == _passwordStep;
        }

        private void OnSendCodeClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            bool isValid = _emailField!.Value.Contains('@');
            _emailError!.Visible = !isValid;
            if (isValid)
            {
                ShowStep(_codeStep!);
            }
        }

        private void OnVerifyClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            bool isValid = _codeField!.Value.Length > 0;
            _codeError!.Visible = !isValid;
            if (isValid)
            {
                ShowStep(_passwordStep!);
            }
        }

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.Login;
        }
    }
}
