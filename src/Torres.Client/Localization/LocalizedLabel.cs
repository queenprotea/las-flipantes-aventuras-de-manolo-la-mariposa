using Myra.Graphics2D.UI;

namespace Torres.Client.Localization
{
    internal sealed class LocalizedLabel : Label, ILocalizedWidget
    {
        private string _textKey;
        private object[]? _textArguments;
        
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
        
        internal object[]? TextArguments
        {
            get => _textArguments;
            set
            {
                _textArguments = value;
                RefreshText();
            }
        }

        public void RefreshText()
        {
            Text = _textArguments is null
                ? LocalizedText.Get(_textKey)
                : LocalizedText.Format(_textKey, _textArguments);
        }
    }
}
