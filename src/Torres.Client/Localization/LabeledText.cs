using System.Globalization;

using Myra.Graphics2D.UI;

using Torres.Client.UI;

namespace Torres.Client.Localization
{
    internal sealed class LabeledText : VerticalStackPanel, ILocalizedWidget
    {
        private readonly string _labelKey;
        private readonly Label _label = new Label();

        internal LabeledText(string labelKey)
        {
            _labelKey = labelKey;
            Spacing = Metrics.FieldLabelSpacing;

            _label.Font = Fonts.Label;
            _label.TextColor = Theme.MutedInk;

            ValueLabel.Font = Fonts.Control;
            ValueLabel.TextColor = Theme.Ink;
            ValueLabel.Wrap = true;

            Widgets.Add(_label);
            Widgets.Add(ValueLabel);
            RefreshText();
        }

        internal Label ValueLabel { get; } = new Label();

        public void RefreshText()
        {
            _label.Text = LocalizedText.Get(_labelKey).ToUpper(CultureInfo.CurrentUICulture);
        }
    }
}
