using System.Globalization;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Ui;

namespace Torres.Client.Localization
{
    internal sealed class LabeledTextBox : VerticalStackPanel, ILocalizedWidget
    {
        private readonly string _labelKey;
        private readonly int _longestText;
        private readonly Label _label = new Label();
        private readonly Label _limitNotice = new Label();

        internal LabeledTextBox(string labelKey, bool isSecret, int longestText)
        {
            _labelKey = labelKey;
            _longestText = longestText;
            Spacing = Metrics.FieldLabelSpacing;

            _label.Font = Fonts.Label;
            _label.TextColor = Theme.MutedInk;

            _limitNotice.Font = Fonts.Small;
            _limitNotice.TextColor = Theme.BlushInk;
            _limitNotice.Wrap = true;
            _limitNotice.Visible = false;

            Box.PasswordField = isSecret;
            Box.Font = Fonts.Control;
            Box.TextColor = Theme.Ink;
            Box.Padding = Metrics.InputPadding;
            Box.Background = Theme.SurfaceSunkenBrush;
            Box.FocusedBackground = Theme.SurfaceBrush;
            Box.Border = Theme.StrongLineBrush;
            Box.BorderThickness = Sizes.Border;
            Box.HorizontalAlignment = HorizontalAlignment.Stretch;
            Box.ValueChanging += BoxOnValueChanging;

            Widgets.Add(_label);
            Widgets.Add(Box);
            Widgets.Add(_limitNotice);
            RefreshText();
        }

        internal string Value => Box.Text ?? string.Empty;

        internal TextBox Box { get; } = new TextBox();

        public void RefreshText()
        {
            _label.Text = LocalizedText.Get(_labelKey).ToUpper(CultureInfo.CurrentUICulture);
            string longestTextNumber = _longestText.ToString(CultureInfo.CurrentCulture);
            _limitNotice.Text = LocalizedText.Format(TextKeys.Common.TextLimitMessage, longestTextNumber);
        }

        private void BoxOnValueChanging(object sender, ValueChangingEventArgs<string> e)
        {
            bool isTooLong = (e.NewValue is not null) && (e.NewValue.Length > _longestText);
            e.Cancel = isTooLong;
            _limitNotice.Visible = isTooLong;
        }
    }
}
