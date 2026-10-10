using System;
using System.Collections.Generic;
using System.Globalization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Torres.LocalizationExample.Localization;
using Torres.LocalizationExample.Resources;

namespace Torres.LocalizationExample.Screens
{
    public sealed class MainMenuScreen : IScreen
    {
        private const float MenuLeft = 60;
        private const float TitleTop = 40;
        private const float FirstButtonTop = 120;
        private const float ButtonSpacing = 50;

        private readonly TorresGame _game;
        private readonly LanguageService _languageService;
        private readonly SpriteFont _uiFont;
        private readonly List<MenuButton> _menuButtons;

        public MainMenuScreen(TorresGame game, LanguageService languageService, SpriteFont uiFont)
        {
            ArgumentNullException.ThrowIfNull(game);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(uiFont);

            _game = game;
            _languageService = languageService;
            _uiFont = uiFont;
            _menuButtons = new List<MenuButton>
            {
                new MenuButton(() => Strings.MainMenu_LogIn, CreateButtonPosition(0), null),
                new MenuButton(() => Strings.MainMenu_Rooms, CreateButtonPosition(1), null),
                new MenuButton(() => Strings.MainMenu_Ranking, CreateButtonPosition(2), null),
                new MenuButton(BuildLanguageButtonText, CreateButtonPosition(3), OpenLanguageScreen),
                new MenuButton(() => Strings.MainMenu_Exit, CreateButtonPosition(4), game.Exit),
            };
        }

        public void Update(MouseState mouseState, bool wasClicked)
        {
            foreach (MenuButton menuButton in _menuButtons)
            {
                menuButton.Update(_uiFont, mouseState.Position, wasClicked);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            ArgumentNullException.ThrowIfNull(spriteBatch);

            spriteBatch.DrawString(_uiFont, Strings.Common_GameName, new Vector2(MenuLeft, TitleTop), Color.LightGreen);
            foreach (MenuButton menuButton in _menuButtons)
            {
                menuButton.Draw(spriteBatch, _uiFont);
            }
        }

        private static Vector2 CreateButtonPosition(int buttonIndex)
        {
            var buttonPosition = new Vector2(MenuLeft, FirstButtonTop + (buttonIndex * ButtonSpacing));

            return buttonPosition;
        }

        private string BuildLanguageButtonText()
        {
            string languageButtonText = string.Format(CultureInfo.CurrentCulture, Strings.MainMenu_Language, _languageService.CurrentLanguage.NativeName);

            return languageButtonText;
        }

        private void OpenLanguageScreen()
        {
            var languageScreen = new ChangeLanguageScreen(_game, _languageService, _uiFont, this);
            _game.ShowScreen(languageScreen);
        }
    }
}
