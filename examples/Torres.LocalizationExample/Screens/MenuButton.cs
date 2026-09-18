using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.LocalizationExample.Screens
{
    /// <summary>
    /// Botón de texto. Recibe una función que devuelve el texto, no el texto: así se relee
    /// del .resx en cada cuadro y el cambio de idioma se ve al instante, incluso en la
    /// pantalla abierta (CU-05, paso 3). Es lo que en WPF haría un binding dinámico;
    /// {x:Static} solo lee el valor una vez.
    /// </summary>
    public sealed class MenuButton
    {
        private readonly Func<string> text;
        private readonly Action onClick;
        private readonly Vector2 position;
        private Rectangle bounds;
        private bool hovered;

        public MenuButton(Func<string> text, Vector2 position, Action onClick)
        {
            this.text = text;
            this.position = position;
            this.onClick = onClick;
        }

        public bool IsMarked { get; set; }

        public void Update(SpriteFont font, Point mouse, bool clicked)
        {
            // El ancho se mide con el texto traducido: "Idioma de la interfaz" e
            // "Interface language" no ocupan lo mismo, así que nada se fija a mano.
            Vector2 size = font.MeasureString(Label());
            bounds = new Rectangle(position.ToPoint(), size.ToPoint());
            hovered = bounds.Contains(mouse);
            if (hovered && clicked)
            {
                onClick();
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            Color color = hovered ? Color.Gold : Color.White;
            spriteBatch.DrawString(font, Label(), position, color);
        }

        private string Label()
        {
            // La marca es "»" (U+00BB) porque la fuente solo incluye Latín-1: un "✓" saldría como "?".
            return IsMarked ? "» " + text() : "   " + text();
        }
    }
}
