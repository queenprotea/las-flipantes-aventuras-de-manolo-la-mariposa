using System.Collections.Generic;

using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Torres.LocalizationExample.Localization;

namespace Torres.LocalizationExample.Screens
{
    public sealed class MainMenuScreen : IScreen
    {
        private readonly TorresGame game;
        private readonly List<MenuButton> buttons;

        public MainMenuScreen(TorresGame game, LanguageService languages)
        {
            this.game = game;
            buttons = new List<MenuButton>
            {
                new MenuButton(() => Strings.MainMenu_LogIn, new Vector2(60, 120), () => { }),
                new MenuButton(() => Strings.MainMenu_Rooms, new Vector2(60, 170), () => { }),
                new MenuButton(() => Strings.MainMenu_Ranking, new Vector2(60, 220), () => { }),
                new MenuButton(
                    () => string.Format(Strings.MainMenu_Language, languages.Current.NativeName),
                    new Vector2(60, 270),
                    () => game.Show(new ChangeLanguageScreen(game, languages, this))),
                new MenuButton(() => Strings.MainMenu_Exit, new Vector2(60, 320), game.Exit),
            };
        }

        public void Update(MouseState mouse, bool clicked)
        {
            foreach (MenuButton button in buttons)
            {
                button.Update(game.Font, mouse.Position, clicked);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(game.Font, "Torres", new Vector2(60, 40), Color.LightGreen);
            foreach (MenuButton button in buttons)
            {
                button.Draw(spriteBatch, game.Font);
            }
        }
    }
}
