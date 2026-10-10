using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Torres.LocalizationExample.Localization;
using Torres.LocalizationExample.Screens;

namespace Torres.LocalizationExample
{
    public sealed class TorresGame : Game
    {
        private const int WindowWidth = 960;
        private const int WindowHeight = 540;
        private const string ContentDirectory = "Content";
        private const string FontAssetName = "Ui";

        private static readonly Color _backgroundColor = new Color(24, 40, 32);

        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private readonly LanguageService _languageService = new LanguageService();

        private SpriteBatch? _spriteBatch;
        private IScreen? _activeScreen;
        private ButtonState _previousLeftButtonState = ButtonState.Released;

        public TorresGame()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = WindowWidth,
                PreferredBackBufferHeight = WindowHeight,
            };
            Content.RootDirectory = ContentDirectory;
            IsMouseVisible = true;
        }

        public void ShowScreen(IScreen nextScreen)
        {
            ArgumentNullException.ThrowIfNull(nextScreen);

            _activeScreen = nextScreen;
        }

        protected override void Initialize()
        {
            _languageService.LoadSavedPreference();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            SpriteFont uiFont = Content.Load<SpriteFont>(FontAssetName);
            _activeScreen = new MainMenuScreen(this, _languageService, uiFont);
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            bool wasClicked = IsActive
                && (mouseState.LeftButton == ButtonState.Released)
                && (_previousLeftButtonState == ButtonState.Pressed);
            _previousLeftButtonState = mouseState.LeftButton;
            _activeScreen?.Update(mouseState, wasClicked);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_backgroundColor);
            DrawActiveScreen();
            base.Draw(gameTime);
        }

        private void DrawActiveScreen()
        {
            if ((_spriteBatch is null) || (_activeScreen is null))
            {
                return;
            }

            _spriteBatch.Begin();
            _activeScreen.Draw(_spriteBatch);
            _spriteBatch.End();
        }
    }
}
