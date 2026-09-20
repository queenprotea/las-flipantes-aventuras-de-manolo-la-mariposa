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

        /// <summary>
        /// Coloca una marca al final del botón, como el chevron del menú principal o el
        /// palomeo del idioma en uso. El rótulo pasa a alinearse a la izquierda.
        /// </summary>
        /// <returns>La marca, para poder mostrarla y ocultarla después.</returns>
        internal Label SetTrailingMark(string mark, Color color)
        {
            _label.HorizontalAlignment = HorizontalAlignment.Left;
            _label.VerticalAlignment = VerticalAlignment.Center;

            var markLabel = new Label
            {
                Text = mark,
                Font = _label.Font,
                TextColor = color,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var content = new Panel { HorizontalAlignment = HorizontalAlignment.Stretch };
            content.Widgets.Add(_label);
            content.Widgets.Add(markLabel);
            Content = content;
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
