using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Torres.LocalizationExample.Localization;
using Torres.LocalizationExample.Resources;

namespace Torres.LocalizationExample.Screens
{
    /// <summary>GUIChangeLanguage (CU-05).</summary>
    public sealed class ChangeLanguageScreen : IScreen
    {
        private readonly TorresGame game;
        private readonly LanguageService languages;
        private readonly List<(MenuButton Button, LanguageOption Language)> options = new();
        private readonly MenuButton backButton;

        public ChangeLanguageScreen(TorresGame game, LanguageService languages, IScreen returnTo)
        {
            this.game = game;
            this.languages = languages;

            float y = 180;
            foreach (LanguageOption language in LanguageService.Available)
            {
                LanguageOption selected = language;
                var button = new MenuButton(
                    () => selected.NativeName, new Vector2(60, y), () => languages.Change(selected));
                options.Add((button, language));
                y += 50;
            }

            // FA01: "Volver" regresa a la ventana de origen.
            backButton = new MenuButton(() => Strings.Common_Back, new Vector2(60, 400), () => game.Show(returnTo));
        }

        public void Update(MouseState mouse, bool clicked)
        {
            foreach ((MenuButton button, LanguageOption language) in options)
            {
                button.Update(game.Font, mouse.Position, clicked);
            }

            // La marca se recalcula después de procesar el clic (paso 4 del flujo normal).
            foreach ((MenuButton button, LanguageOption language) in options)
            {
                button.IsMarked = language.CultureCode == languages.Current.CultureCode;
            }

            backButton.Update(game.Font, mouse.Position, clicked);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(game.Font, Strings.ChangeLanguage_Title, new Vector2(60, 40), Color.LightGreen);
            spriteBatch.DrawString(game.Font, Strings.ChangeLanguage_Hint, new Vector2(60, 90), Color.LightGray);
            foreach ((MenuButton button, LanguageOption language) in options)
            {
                button.Draw(spriteBatch, game.Font);
            }

            spriteBatch.DrawString(game.Font, Strings.ChangeLanguage_Note, new Vector2(60, 320), Color.LightGray);
            backButton.Draw(spriteBatch, game.Font);
        }
    }
}
