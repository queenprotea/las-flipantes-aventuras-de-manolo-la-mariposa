using Myra.Graphics2D.UI;

namespace Torres.Client.Localization
{
    internal sealed class LocalizedSymbolButton : Button, ILocalizedWidget
    {
        private readonly string _tooltipKey;

        internal LocalizedSymbolButton(Label symbolLabel, string tooltipKey)
        {
            _tooltipKey = tooltipKey;
            Content = symbolLabel;
            RefreshText();
        }

        public void RefreshText()
        {
            Tooltip = LocalizedText.Get(_tooltipKey);
        }
    }
}
