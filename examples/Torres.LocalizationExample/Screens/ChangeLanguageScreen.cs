using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Torres.LocalizationExample.Localization;
using Torres.LocalizationExample.Resources;

namespace Torres.LocalizationExample.Screens
{
    public sealed class ChangeLanguageScreen : IScreen
    {
        private const float ScreenLeft = 60;
        private const float TitleTop = 40;
        private const float HintTop = 90;
        private const float FirstLanguageTop = 180;
        private const float LanguageSpacing = 50;
        private const float NoteTop = 320;
        private const float BackButtonTop = 400;

        private readonly TorresGame _game;
        private readonly LanguageService _languageService;
        private readonly SpriteFont _uiFont;
        private readonly IScreen _returnScreen;
        private readonly List<(MenuButton Button, LanguageOption Language)> _languageButtons =
            new List<(MenuButton Button, LanguageOption Language)>();
        private readonly MenuButton _backButton;

        public ChangeLanguageScreen(TorresGame game, LanguageService languageService, SpriteFont uiFont, IScreen returnScreen)
        {
            ArgumentNullException.ThrowIfNull(game);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(uiFont);
            ArgumentNullException.ThrowIfNull(returnScreen);

            _game = game;
            _languageService = languageService;
            _uiFont = uiFont;
            _returnScreen = returnScreen;
            float languageButtonTop = FirstLanguageTop;
            foreach (LanguageOption availableLanguage in LanguageService.AvailableLanguages)
            {
                var languageButton = new MenuButton(
                    () => availableLanguage.NativeName,
                    new Vector2(ScreenLeft, languageButtonTop),
                    () => languageService.Change(availableLanguage));
                _languageButtons.Add((languageButton, availableLanguage));
                languageButtonTop += LanguageSpacing;
            }

            _backButton = new MenuButton(() => Strings.Common_Back, new Vector2(ScreenLeft, BackButtonTop), ReturnToPreviousScreen);
        }

        public void Update(MouseState mouseState, bool wasClicked)
        {
            foreach ((MenuButton languageButton, _) in _languageButtons)
            {
                languageButton.Update(_uiFont, mouseState.Position, wasClicked);
            }

            foreach ((MenuButton languageButton, LanguageOption buttonLanguage) in _languageButtons)
            {
                languageButton.IsMarked = buttonLanguage.CultureCode == _languageService.CurrentLanguage.CultureCode;
            }

            _backButton.Update(_uiFont, mouseState.Position, wasClicked);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            ArgumentNullException.ThrowIfNull(spriteBatch);

            spriteBatch.DrawString(_uiFont, Strings.ChangeLanguage_Title, new Vector2(ScreenLeft, TitleTop), Color.LightGreen);
            spriteBatch.DrawString(_uiFont, Strings.ChangeLanguage_Hint, new Vector2(ScreenLeft, HintTop), Color.LightGray);
            foreach ((MenuButton languageButton, _) in _languageButtons)
            {
                languageButton.Draw(spriteBatch, _uiFont);
            }

            spriteBatch.DrawString(_uiFont, Strings.ChangeLanguage_Note, new Vector2(ScreenLeft, NoteTop), Color.LightGray);
            _backButton.Draw(spriteBatch, _uiFont);
        }

        private void ReturnToPreviousScreen()
        {
            _game.ShowScreen(_returnScreen);
        }
    }
}
