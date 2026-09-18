using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Torres.LocalizationExample.Screens
{
    public interface IScreen
    {
        void Update(MouseState mouse, bool clicked);

        void Draw(SpriteBatch spriteBatch);
    }
}
