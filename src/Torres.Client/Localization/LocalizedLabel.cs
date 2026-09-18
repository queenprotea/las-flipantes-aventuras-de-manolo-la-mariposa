using Myra.Graphics2D.UI;

namespace Torres.Client.Localization
{
    internal sealed class LocalizedLabel : Label, ILocalizedWidget
    {
        private string _textKey;

        internal LocalizedLabel(string textKey)
        {
            _textKey = textKey;
            RefreshText();
        }

        internal string TextKey
        {
            get => _textKey;
            set
            {
                _textKey = value;
                RefreshText();
            }
        }

        public void RefreshText()
        {
            Text = LocalizedText.Get(_textKey);
        }
    }
}
