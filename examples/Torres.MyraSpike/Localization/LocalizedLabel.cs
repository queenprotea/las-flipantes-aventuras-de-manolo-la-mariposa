using Myra.Graphics2D.UI;

namespace Torres.MyraSpike.Localization
{
    internal sealed class LocalizedLabel : Label, ILocalizedWidget
    {
        private readonly string _textKey;

        internal LocalizedLabel(string textKey)
        {
            _textKey = textKey;
            RefreshText();
        }

        public void RefreshText()
        {
            Text = LocalizedText.Get(_textKey);
        }
    }
}
