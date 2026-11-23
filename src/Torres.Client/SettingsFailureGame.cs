using Microsoft.Xna.Framework;

using Myra;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Screens;
using Torres.Client.Ui;

namespace Torres.Client
{
    internal sealed class SettingsFailureGame : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private readonly LanguagePreference _languagePreference = new LanguagePreference();
        private readonly SettingsFailureScreen _settingsFailureScreen = new SettingsFailureScreen();

        private Desktop? _desktop;
        private string _appliedUiCultureName = string.Empty;

        internal SettingsFailureGame()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            _graphicsDeviceManager.PreferredBackBufferWidth = Sizes.WindowWidth;
            _graphicsDeviceManager.PreferredBackBufferHeight = Sizes.WindowHeight;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _languagePreference.StartLoadingSavedPreference();
            _appliedUiCultureName = _languagePreference.CurrentLanguage.UiCultureName;
            MyraEnvironment.Game = this;
            _desktop = new Desktop();
            _desktop.Root = _settingsFailureScreen.Root;
            LanguageRefresher.Refresh(_settingsFailureScreen.Root);
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            _languagePreference.Update();
            FollowLanguageChange();
            Window.Title = LocalizedText.Get(TextKeys.Common.WindowTitle);
            if (_settingsFailureScreen.WasExitRequested)
            {
                Exit();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Theme.Surface);
            _desktop?.Render();
            base.Draw(gameTime);
        }

        private void FollowLanguageChange()
        {
            if (_languagePreference.CurrentLanguage.UiCultureName == _appliedUiCultureName)
            {
                return;
            }

            _appliedUiCultureName = _languagePreference.CurrentLanguage.UiCultureName;
            LanguageRefresher.Refresh(_settingsFailureScreen.Root);
        }
    }
}
