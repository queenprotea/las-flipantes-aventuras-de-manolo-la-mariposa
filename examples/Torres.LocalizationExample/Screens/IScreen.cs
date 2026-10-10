using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Torres.LocalizationExample.Screens
{
    public interface IScreen
    {
        void Update(MouseState mouseState, bool wasClicked);

        void Draw(SpriteBatch spriteBatch);
    }
}
