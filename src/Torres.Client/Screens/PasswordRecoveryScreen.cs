using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class PasswordRecoveryScreen : Screen
    {
        private readonly VerticalStackPanel _emailStep = Card(Sizes.RecoveryCardWidth);
        private readonly VerticalStackPanel _codeStep = Card(Sizes.RecoveryCardWidth);
        private readonly VerticalStackPanel _passwordStep = Card(Sizes.RecoveryCardWidth);
        private readonly LabeledTextBox _emailField = new LabeledTextBox(TextKeys.PasswordRecovery.EmailLabel, false);
        private readonly LabeledTextBox _codeField = new LabeledTextBox(TextKeys.PasswordRecovery.CodeLabel, false);
        private readonly LocalizedLabel _emailError = Error(TextKeys.PasswordRecovery.InvalidEmail);
        private readonly LocalizedLabel _codeError = Error(TextKeys.PasswordRecovery.InvalidCode);

        internal PasswordRecoveryScreen()
            : base(TextKeys.PasswordRecovery.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            var steps = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
            };
            steps.Widgets.Add(BuildEmailStep());
            steps.Widgets.Add(BuildCodeStep());
            steps.Widgets.Add(BuildPasswordStep());

            ShowStep(_emailStep);

            return steps;
        }

        private VerticalStackPanel BuildEmailStep()
        {
            var field = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            field.Widgets.Add(_emailField);
            field.Widgets.Add(_emailError);

            LocalizedButton sendCodeButton = PrimaryButton(TextKeys.PasswordRecovery.SendCodeButton);
            sendCodeButton.Click += SendCodeButtonOnClick;
            sendCodeButton.HorizontalAlignment = HorizontalAlignment.Left;
            VerticalStackPanel actions = WrappedRow();
            actions.Widgets.Add(sendCodeButton);
            actions.Widgets.Add(BackToLoginButton());

            _emailStep.HorizontalAlignment = HorizontalAlignment.Left;
            _emailStep.Widgets.Add(CardHeader(TextKeys.PasswordRecovery.Step1Title, TextKeys.PasswordRecovery.Step1Hint));
            _emailStep.Widgets.Add(field);
            _emailStep.Widgets.Add(actions);

            return _emailStep;
        }

        private VerticalStackPanel BuildCodeStep()
        {
            var field = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            field.Widgets.Add(_codeField);
            field.Widgets.Add(_codeError);

            LocalizedButton verifyButton = PrimaryButton(TextKeys.PasswordRecovery.VerifyButton);
            verifyButton.Click += VerifyButtonOnClick;
            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(verifyButton);

            _codeStep.HorizontalAlignment = HorizontalAlignment.Left;
            _codeStep.Visible = false;
            _codeStep.Widgets.Add(CardHeader(TextKeys.PasswordRecovery.Step2Title, TextKeys.PasswordRecovery.Step2Hint));
            _codeStep.Widgets.Add(field);
            _codeStep.Widgets.Add(actions);

            return _codeStep;
        }

        private VerticalStackPanel BuildPasswordStep()
        {
            LocalizedButton saveButton = PrimaryButton(TextKeys.PasswordRecovery.SaveButton);
            saveButton.HorizontalAlignment = HorizontalAlignment.Left;
            VerticalStackPanel actions = WrappedRow();
            actions.Widgets.Add(saveButton);
            actions.Widgets.Add(BackToLoginButton());

            _passwordStep.HorizontalAlignment = HorizontalAlignment.Left;
            _passwordStep.Visible = false;
            _passwordStep.Widgets.Add(CardHeader(TextKeys.PasswordRecovery.Step3Title, TextKeys.PasswordRecovery.Step3Hint));
            _passwordStep.Widgets.Add(new LabeledTextBox(TextKeys.PasswordRecovery.NewPasswordLabel, true));
            _passwordStep.Widgets.Add(new LabeledTextBox(TextKeys.PasswordRecovery.RepeatPasswordLabel, true));
            _passwordStep.Widgets.Add(actions);

            return _passwordStep;
        }

        private LocalizedButton BackToLoginButton()
        {
            LocalizedButton backButton = SecondaryButton(TextKeys.PasswordRecovery.BackToLoginButton);
            backButton.HorizontalAlignment = HorizontalAlignment.Left;
            backButton.Click += BackButtonOnClick;

            return backButton;
        }

        private void ShowStep(VerticalStackPanel step)
        {
            _emailStep.Visible = step == _emailStep;
            _codeStep.Visible = step == _codeStep;
            _passwordStep.Visible = step == _passwordStep;
        }

        private void SendCodeButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            bool isValid = _emailField.Value.Contains('@');
            _emailError.Visible = !isValid;
            if (isValid)
            {
                ShowStep(_codeStep);
            }
        }

        private void VerifyButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            bool isValid = _codeField.Value.Length > 0;
            _codeError.Visible = !isValid;
            if (isValid)
            {
                ShowStep(_passwordStep);
            }
        }

        private void BackButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Login;
        }
    }
}
