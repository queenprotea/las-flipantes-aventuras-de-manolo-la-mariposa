using System.Globalization;

using Myra.Graphics2D.UI;

using Torres.Client.Ui;

namespace Torres.Client.Localization
{
    internal sealed class LabeledTextBox : VerticalStackPanel, ILocalizedWidget
    {
        private readonly string _labelKey;
        private readonly Label _label = new Label();

        internal LabeledTextBox(string labelKey, bool isSecret)
        {
            _labelKey = labelKey;
            Spacing = Metrics.FieldLabelSpacing;

            _label.Font = Fonts.Label;
            _label.TextColor = Theme.MutedInk;

            Box.PasswordField = isSecret;
            Box.Font = Fonts.Control;
            Box.TextColor = Theme.Ink;
            Box.Padding = Metrics.InputPadding;
            Box.Background = Theme.SurfaceSunkenBrush;
            Box.FocusedBackground = Theme.SurfaceBrush;
            Box.Border = Theme.StrongLineBrush;
            Box.BorderThickness = Sizes.Border;
            Box.HorizontalAlignment = HorizontalAlignment.Stretch;

            Widgets.Add(_label);
            Widgets.Add(Box);
            RefreshText();
        }

        internal string Value => Box.Text ?? string.Empty;

        internal TextBox Box { get; } = new TextBox();

        public void RefreshText()
        {
            _label.Text = LocalizedText.Get(_labelKey).ToUpper(CultureInfo.CurrentUICulture);
        }
    }
}
