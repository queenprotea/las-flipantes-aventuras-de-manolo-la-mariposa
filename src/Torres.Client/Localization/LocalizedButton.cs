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

        public void RefreshText()
        {
            _label.Text = TextArgumentKey is null
                ? LocalizedText.Get(_textKey)
                : LocalizedText.Format(_textKey, LocalizedText.Get(TextArgumentKey));
        }
    }
}
