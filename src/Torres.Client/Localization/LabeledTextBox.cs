using System;
using System.Globalization;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.UI;
using Torres.Client.Widgets;

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
            Box = new LimitedTextBox(longestText);
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
            Box.TextChanged += BoxOnTextChanged;
            Box.LimitReached += BoxOnLimitReached;

            Widgets.Add(_label);
            Widgets.Add(Box);
            Widgets.Add(_limitNotice);
            RefreshText();
        }

        internal string Value => Box.Text ?? string.Empty;

        internal LimitedTextBox Box { get; }

        public void RefreshText()
        {
            _label.Text = LocalizedText.Get(_labelKey).ToUpper(CultureInfo.CurrentUICulture);
            string longestTextNumber = _longestText.ToString(CultureInfo.CurrentCulture);
            _limitNotice.Text = LocalizedText.Format(TextKeys.Common.TextLimitMessage, longestTextNumber);
        }

        private void BoxOnTextChanged(object sender, ValueChangedEventArgs<string> e)
        {
            _limitNotice.Visible = false;
        }

        private void BoxOnLimitReached(object? sender, EventArgs e)
        {
            _limitNotice.Visible = true;
        }
    }
}
