using System;
using System.Globalization;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Myra;
using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.MyraSpike.Localization;

namespace Torres.MyraSpike
{
    internal sealed class MyraSpikeGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly bool _capturing;
        private Desktop? _desktop;
        private Label? _reportLabel;
        private int _lastRefreshedCount;
        private int _frames;

        internal MyraSpikeGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 900;
            _graphics.PreferredBackBufferHeight = 660;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _capturing = Environment.GetCommandLineArgs().Length > 1;
        }

        protected override void Initialize()
        {
            ApplyLanguage("es", "es-MX");
            MyraEnvironment.Game = this;

            _desktop = new Desktop();
            _desktop.Root = Build();
            UpdateReport();
            base.Initialize();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(18, 26, 22));
            _desktop!.Render();
            base.Draw(gameTime);

            if (_capturing)
            {
                Capture();
            }
        }

        private Widget Build()
        {
            _reportLabel = new Label();
            var root = new VerticalStackPanel { Spacing = 10, Margin = new Thickness(24) };

            root.Widgets.Add(new LocalizedLabel(TextKeys.Language.Title));
            root.Widgets.Add(new LocalizedLabel(TextKeys.Language.Hint) { Wrap = true });
            root.Widgets.Add(new LocalizedLabel(TextKeys.Common.WindowTitle));
            root.Widgets.Add(new LocalizedLabel(TextKeys.Startup.ConnectingStatus));

            var buttons = new HorizontalStackPanel { Spacing = 8 };
            var spanishButton = new LocalizedButton(TextKeys.Language.SpanishOption);
            var englishButton = new LocalizedButton(TextKeys.Language.EnglishOption);
            spanishButton.Click += OnSpanishClick;
            englishButton.Click += OnEnglishClick;
            buttons.Widgets.Add(spanishButton);
            buttons.Widgets.Add(englishButton);
            root.Widgets.Add(buttons);

            root.Widgets.Add(new LocalizedLabel(TextKeys.Login.UsernameLabel));
            root.Widgets.Add(new TextBox { Width = 320 });
            root.Widgets.Add(new LocalizedLabel(TextKeys.Login.PasswordLabel));
            root.Widgets.Add(new TextBox { Width = 320, PasswordField = true });
            root.Widgets.Add(new LocalizedLabel(TextKeys.GuestAccess.Limitations) { Wrap = true, Width = 620 });

            var rows = new VerticalStackPanel { Spacing = 4 };
            foreach (string key in ScrollKeys())
            {
                rows.Widgets.Add(new LocalizedLabel(key));
            }

            root.Widgets.Add(new ScrollViewer { Content = rows, Height = 120, Width = 620 });
            root.Widgets.Add(_reportLabel);
            return root;
        }

        private static string[] ScrollKeys()
        {
            return new[]
            {
                TextKeys.MainMenu.RoomsButton, TextKeys.MainMenu.InvitationsButton, TextKeys.MainMenu.FriendsButton,
                TextKeys.MainMenu.HistoryButton, TextKeys.MainMenu.RankingButton, TextKeys.MainMenu.ExitButton,
                TextKeys.Login.Title, TextKeys.Register.Title, TextKeys.GuestAccess.Title,
                TextKeys.Profile.AccountTitle, TextKeys.AccountSettings.DeleteAccountTitle,
                TextKeys.PasswordRecovery.Step1Title, TextKeys.PasswordRecovery.Step2Title,
                TextKeys.PasswordRecovery.Step3Title, TextKeys.Common.RetryButton, TextKeys.Common.CancelButton,
                TextKeys.Common.GotItButton, TextKeys.Common.BackToMenuButton, TextKeys.Common.OfflineStatus,
            };
        }

        private void OnSpanishClick(object sender, MyraEventArgs arguments)
        {
            SwitchLanguage("es", "es-MX");
        }

        private void OnEnglishClick(object sender, MyraEventArgs arguments)
        {
            SwitchLanguage("en", "en-US");
        }

        private void SwitchLanguage(string uiCultureName, string formatCultureName)
        {
            ApplyLanguage(uiCultureName, formatCultureName);
            _lastRefreshedCount = LanguageRefresher.Refresh(_desktop!.Root);
            UpdateReport();
        }

        private static void ApplyLanguage(string uiCultureName, string formatCultureName)
        {
            CultureInfo uiCulture = CultureInfo.GetCultureInfo(uiCultureName);
            CultureInfo formatCulture = CultureInfo.GetCultureInfo(formatCultureName);
            CultureInfo.CurrentUICulture = uiCulture;
            CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
            CultureInfo.CurrentCulture = formatCulture;
            CultureInfo.DefaultThreadCurrentCulture = formatCulture;
        }

        private void UpdateReport()
        {
            string date = new DateTime(2026, 9, 5).ToString("dd MMM yyyy", CultureInfo.CurrentCulture);
            string number = 1480.5.ToString("N1", CultureInfo.CurrentCulture);
            _reportLabel!.Text = $"UI={CultureInfo.CurrentUICulture.Name}  Cultura={CultureInfo.CurrentCulture.Name}  " +
                $"{date}  {number}  widgets refrescados={_lastRefreshedCount}";
        }

        private void Capture()
        {
            _frames += 1;
            if (_frames == 3)
            {
                Save("myra-es.png");
            }

            if (_frames == 4)
            {
                SwitchLanguage("en", "en-US");
            }

            if (_frames == 7)
            {
                Save("myra-en.png");
                Exit();
            }
        }

        private void Save(string fileName)
        {
            int width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int height = GraphicsDevice.PresentationParameters.BackBufferHeight;
            var target = new RenderTarget2D(GraphicsDevice, width, height);
            GraphicsDevice.SetRenderTarget(target);
            GraphicsDevice.Clear(new Color(18, 26, 22));
            _desktop!.Render();
            GraphicsDevice.SetRenderTarget(null);

            using (FileStream stream = File.Create(fileName))
            {
                target.SaveAsPng(stream, width, height);
            }

            target.Dispose();
            Console.WriteLine("guardado " + Path.GetFullPath(fileName));
        }
    }
}
