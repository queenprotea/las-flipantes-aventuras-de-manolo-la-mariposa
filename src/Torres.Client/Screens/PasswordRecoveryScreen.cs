using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    /// <summary>
    /// PT-05 del prototipo: los tres pasos de la recuperación, cada uno en su tarjeta de 290px.
    /// El prototipo los dibuja juntos para poder verlos; aquí se muestra el que toca.
    /// </summary>
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
            // Las tres tarjetas ocupan el mismo sitio: solo una está visible cada vez.
            var steps = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
            };
            steps.Widgets.Add(BuildEmailStep());
            steps.Widgets.Add(BuildCodeStep());
            steps.Widgets.Add(BuildPasswordStep());

            ShowStep(_emailStep!);
            return steps;
        }

        private VerticalStackPanel BuildEmailStep()
        {
            var field = new VerticalStackPanel { Spacing = Theme.FieldLabelSpacing };
            _emailField = new LabeledTextBox(TextKeys.PasswordRecovery.EmailLabel, false);
            _emailError = Error(TextKeys.PasswordRecovery.InvalidEmail);
            field.Widgets.Add(_emailField);
            field.Widgets.Add(_emailError);

            LocalizedButton sendCodeButton = PrimaryButton(TextKeys.PasswordRecovery.SendCodeButton);
            sendCodeButton.Click += OnSendCodeClick;
            sendCodeButton.HorizontalAlignment = HorizontalAlignment.Left;
            VerticalStackPanel actions = WrappedRow();
            actions.Widgets.Add(sendCodeButton);
            actions.Widgets.Add(BackToLoginButton());

            _emailStep = Card(Theme.RecoveryCardWidth);
            _emailStep.HorizontalAlignment = HorizontalAlignment.Left;
            _emailStep.Widgets.Add(CardHeader(TextKeys.PasswordRecovery.Step1Title, TextKeys.PasswordRecovery.Step1Hint));
            _emailStep.Widgets.Add(field);
            _emailStep.Widgets.Add(actions);
            return _emailStep;
        }

        private VerticalStackPanel BuildCodeStep()
        {
            var field = new VerticalStackPanel { Spacing = Theme.FieldLabelSpacing };
            _codeField = new LabeledTextBox(TextKeys.PasswordRecovery.CodeLabel, false);
            _codeError = Error(TextKeys.PasswordRecovery.InvalidCode);
            field.Widgets.Add(_codeField);
            field.Widgets.Add(_codeError);

            LocalizedButton verifyButton = PrimaryButton(TextKeys.PasswordRecovery.VerifyButton);
            verifyButton.Click += OnVerifyClick;
            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(verifyButton);

            _codeStep = Card(Theme.RecoveryCardWidth);
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

            _passwordStep = Card(Theme.RecoveryCardWidth);
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
            backButton.Click += OnBackClick;
            return backButton;
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
