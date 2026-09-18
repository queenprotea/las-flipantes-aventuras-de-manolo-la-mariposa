using Myra.Graphics2D.UI;

namespace Torres.MyraSpike.Localization
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

        public void RefreshText()
        {
            _label.Text = LocalizedText.Get(_textKey);
        }
    }
}
