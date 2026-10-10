using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.LocalizationExample.Screens
{
    public sealed class MenuButton
    {
        private const string MarkedPrefix = "» ";
        private const string UnmarkedPrefix = "   ";

        private readonly Func<string> _textProvider;
        private readonly Vector2 _position;
        private readonly Action? _clickAction;

        private bool _isHovered;

        public MenuButton(Func<string> textProvider, Vector2 position, Action? clickAction)
        {
            _textProvider = textProvider;
            _position = position;
            _clickAction = clickAction;
        }

        public bool IsMarked { get; set; }

        public void Update(SpriteFont uiFont, Point mousePosition, bool wasClicked)
        {
            ArgumentNullException.ThrowIfNull(uiFont);

            string buttonText = BuildButtonText();
            Vector2 textSize = uiFont.MeasureString(buttonText);
            var buttonBounds = new Rectangle(_position.ToPoint(), textSize.ToPoint());
            _isHovered = buttonBounds.Contains(mousePosition);
            if (_isHovered && wasClicked)
            {
                _clickAction?.Invoke();
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont uiFont)
        {
            ArgumentNullException.ThrowIfNull(spriteBatch);
            ArgumentNullException.ThrowIfNull(uiFont);

            Color textColor = _isHovered ? Color.Gold : Color.White;
            spriteBatch.DrawString(uiFont, BuildButtonText(), _position, textColor);
        }

        private string BuildButtonText()
        {
            string markPrefix = IsMarked ? MarkedPrefix : UnmarkedPrefix;
            string buttonText = markPrefix + _textProvider();

            return buttonText;
        }
    }
}
