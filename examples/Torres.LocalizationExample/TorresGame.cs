using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Torres.LocalizationExample.Localization;
using Torres.LocalizationExample.Screens;

namespace Torres.LocalizationExample
{
    public sealed class TorresGame : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private readonly LanguageService languages = new();
        private SpriteBatch spriteBatch = null!;
        private IScreen screen = null!;
        private ButtonState previousLeftButton = ButtonState.Released;

        public TorresGame()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 960,
                PreferredBackBufferHeight = 540,
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        public SpriteFont Font { get; private set; } = null!;

        public void Show(IScreen next)
        {
            screen = next;
        }

        protected override void Initialize()
        {
            // Antes de crear cualquier pantalla, como en el constructor de App.xaml.cs.
            languages.LoadSavedPreference();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Font = Content.Load<SpriteFont>("Ui");
            screen = new MainMenuScreen(this, languages);
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();
            bool clicked = IsActive
                && mouse.LeftButton == ButtonState.Released
                && previousLeftButton == ButtonState.Pressed;
            previousLeftButton = mouse.LeftButton;

            screen.Update(mouse, clicked);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(24, 40, 32));
            spriteBatch.Begin();
            screen.Draw(spriteBatch);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
