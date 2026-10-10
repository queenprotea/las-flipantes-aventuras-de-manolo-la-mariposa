using FontStashSharp;

using Microsoft.Xna.Framework;

using Myra.Graphics2D.UI;

namespace Torres.Client.Localization
{
    internal sealed class LocalizedButton : Button, ILocalizedWidget
    {
        private readonly string _textKey;
        private readonly Label _label = new Label();

        internal LocalizedButton(string textKey)
        {
            _textKey = textKey;
            Content = _label;
            RefreshText();
        }

        internal string? TextArgumentKey { get; set; }

        internal Color LabelColor
        {
            get => _label.TextColor;
            set => _label.TextColor = value;
        }

        internal SpriteFontBase LabelFont
        {
            get => _label.Font;
            set => _label.Font = value;
        }

        internal HorizontalAlignment LabelAlignment
        {
            get => _label.HorizontalAlignment;
            set => _label.HorizontalAlignment = value;
        }

        internal Label SetTrailingMark(string markText, Color markColor)
        {
            _label.HorizontalAlignment = HorizontalAlignment.Left;
            _label.VerticalAlignment = VerticalAlignment.Center;

            var markLabel = new Label
            {
                Text = markText,
                Font = _label.Font,
                TextColor = markColor,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var labelWithMark = new Panel { HorizontalAlignment = HorizontalAlignment.Stretch };
            labelWithMark.Widgets.Add(_label);
            labelWithMark.Widgets.Add(markLabel);
            Content = labelWithMark;

            return markLabel;
        }

        public void RefreshText()
        {
            _label.Text = TextArgumentKey is null
                ? LocalizedText.Get(_textKey)
                : LocalizedText.Format(_textKey, LocalizedText.Get(TextArgumentKey));
        }
    }
}
