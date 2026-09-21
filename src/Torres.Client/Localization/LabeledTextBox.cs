using System.Globalization;

using Myra.Graphics2D.UI;

using Torres.Client.Ui;

namespace Torres.Client.Localization
{
    internal sealed class LabeledTextBox : VerticalStackPanel, ILocalizedWidget
    {
        private readonly string _labelKey;
        private readonly Label _label = new Label();
        private readonly TextBox _box = new TextBox();

        internal LabeledTextBox(string labelKey, bool isSecret)
        {
            _labelKey = labelKey;
            Spacing = Theme.FieldLabelSpacing;

            _label.Font = Fonts.Label;
            _label.TextColor = Theme.MutedInk;

            _box.PasswordField = isSecret;
            _box.Font = Fonts.Control;
            _box.TextColor = Theme.Ink;
            _box.Padding = Theme.InputPadding;
            _box.Background = Theme.SurfaceSunkenBrush;
            _box.FocusedBackground = Theme.SurfaceBrush;
            _box.Border = Theme.StrongLineBrush;
            _box.BorderThickness = Theme.Border;
            _box.HorizontalAlignment = HorizontalAlignment.Stretch;

            Widgets.Add(_label);
            Widgets.Add(_box);
            RefreshText();
        }

        internal string Value => _box.Text ?? string.Empty;

        internal TextBox Box => _box;

        public void RefreshText()
        {
            _label.Text = LocalizedText.Get(_labelKey).ToUpper(CultureInfo.CurrentUICulture);
        }
    }
}
