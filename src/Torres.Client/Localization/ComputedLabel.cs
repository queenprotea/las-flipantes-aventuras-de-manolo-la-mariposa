using System;

using Myra.Graphics2D.UI;

namespace Torres.Client.Localization
{
    internal sealed class ComputedLabel : Label, ILocalizedWidget
    {
        private readonly Func<string> _textProvider;

        internal ComputedLabel(Func<string> textProvider)
        {
            ArgumentNullException.ThrowIfNull(textProvider);

            _textProvider = textProvider;
            RefreshText();
        }

        public void RefreshText()
        {
            Text = _textProvider();
        }
    }
}
