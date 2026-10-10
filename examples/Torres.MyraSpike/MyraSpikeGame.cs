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
        private const int WindowWidth = 900;
        private const int WindowHeight = 660;
        private const string ContentDirectory = "Content";
        private const string SpanishUiCulture = "es";
        private const string SpanishFormatCulture = "es-MX";
        private const string EnglishUiCulture = "en";
        private const string EnglishFormatCulture = "en-US";
        private const int RootSpacing = 10;
        private const int RootMargin = 24;
        private const int ButtonSpacing = 8;
        private const int TextBoxWidth = 320;
        private const int WideContentWidth = 620;
        private const int ScrollRowSpacing = 4;
        private const int ScrollViewerHeight = 120;
        private const int SpanishCaptureFrame = 3;
        private const int LanguageSwitchFrame = 4;
        private const int EnglishCaptureFrame = 7;
        private const string SpanishCaptureFile = "myra-es.png";
        private const string EnglishCaptureFile = "myra-en.png";
        private const string SampleDatePattern = "dd MMM yyyy";
        private const string SampleNumberPattern = "N1";
        private const double SampleNumber = 1480.5;
        private const string ReportPattern = "UI={0}  Cultura={1}  {2}  {3}  widgets refrescados={4}";
        private const string SavedCaptureMessage = "guardado {0}";

        private static readonly Color _backgroundColor = new Color(18, 26, 22);
        private static readonly DateTime _sampleDate = new DateTime(2026, 9, 5);

        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private readonly bool _isCapturing;
        private Desktop? _desktop;
        private Label? _reportLabel;
        private int _lastRefreshedWidgetCount;
        private int _drawnFrameCount;

        internal MyraSpikeGame()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            _graphicsDeviceManager.PreferredBackBufferWidth = WindowWidth;
            _graphicsDeviceManager.PreferredBackBufferHeight = WindowHeight;
            Content.RootDirectory = ContentDirectory;
            IsMouseVisible = true;
            _isCapturing = Environment.GetCommandLineArgs().Length > 1;
        }

        protected override void Initialize()
        {
            ApplyLanguage(SpanishUiCulture, SpanishFormatCulture);
            MyraEnvironment.Game = this;
            _reportLabel = new Label();
            _desktop = new Desktop { Root = BuildLayout(_reportLabel) };
            UpdateReport();
            base.Initialize();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_backgroundColor);
            _desktop?.Render();
            base.Draw(gameTime);
            if (_isCapturing)
            {
                AdvanceCapture();
            }
        }

        private Widget BuildLayout(Label reportLabel)
        {
            var spikeLayout = new VerticalStackPanel { Spacing = RootSpacing, Margin = new Thickness(RootMargin) };
            spikeLayout.Widgets.Add(new LocalizedLabel(TextKeys.Language.Title));
            spikeLayout.Widgets.Add(new LocalizedLabel(TextKeys.Language.Hint) { Wrap = true });
            spikeLayout.Widgets.Add(new LocalizedLabel(TextKeys.Common.WindowTitle));
            spikeLayout.Widgets.Add(new LocalizedLabel(TextKeys.Startup.ConnectingStatus));
            spikeLayout.Widgets.Add(BuildLanguageButtons());
            spikeLayout.Widgets.Add(new LocalizedLabel(TextKeys.Login.UsernameLabel));
            spikeLayout.Widgets.Add(new TextBox { Width = TextBoxWidth });
            spikeLayout.Widgets.Add(new LocalizedLabel(TextKeys.Login.PasswordLabel));
            spikeLayout.Widgets.Add(new TextBox { Width = TextBoxWidth, PasswordField = true });
            spikeLayout.Widgets.Add(new LocalizedLabel(TextKeys.GuestAccess.Limitations) { Wrap = true, Width = WideContentWidth });
            spikeLayout.Widgets.Add(BuildScrollableKeys());
            spikeLayout.Widgets.Add(reportLabel);

            return spikeLayout;
        }

        private HorizontalStackPanel BuildLanguageButtons()
        {
            var spanishButton = new LocalizedButton(TextKeys.Language.SpanishOption);
            var englishButton = new LocalizedButton(TextKeys.Language.EnglishOption);
            spanishButton.Click += SpanishButtonOnClick;
            englishButton.Click += EnglishButtonOnClick;

            var languageButtons = new HorizontalStackPanel { Spacing = ButtonSpacing };
            languageButtons.Widgets.Add(spanishButton);
            languageButtons.Widgets.Add(englishButton);

            return languageButtons;
        }

        private static ScrollViewer BuildScrollableKeys()
        {
            var keyRows = new VerticalStackPanel { Spacing = ScrollRowSpacing };
            foreach (string textKey in CreateScrollKeys())
            {
                keyRows.Widgets.Add(new LocalizedLabel(textKey));
            }

            var scrollableKeys = new ScrollViewer { Content = keyRows, Height = ScrollViewerHeight, Width = WideContentWidth };

            return scrollableKeys;
        }

        private static string[] CreateScrollKeys()
        {
            string[] scrollKeys =
            {
                TextKeys.MainMenu.RoomsButton,
                TextKeys.MainMenu.InvitationsButton,
                TextKeys.MainMenu.FriendsButton,
                TextKeys.MainMenu.HistoryButton,
                TextKeys.MainMenu.RankingButton,
                TextKeys.MainMenu.ExitButton,
                TextKeys.Login.Title,
                TextKeys.Register.Title,
                TextKeys.GuestAccess.Title,
                TextKeys.Profile.AccountTitle,
                TextKeys.AccountSettings.DeleteAccountTitle,
                TextKeys.PasswordRecovery.Step1Title,
                TextKeys.PasswordRecovery.Step2Title,
                TextKeys.PasswordRecovery.Step3Title,
                TextKeys.Common.RetryButton,
                TextKeys.Common.CancelButton,
                TextKeys.Common.GotItButton,
                TextKeys.Common.BackToMenuButton,
                TextKeys.Common.OfflineStatus,
            };

            return scrollKeys;
        }

        private void SpanishButtonOnClick(object sender, MyraEventArgs e)
        {
            SwitchLanguage(SpanishUiCulture, SpanishFormatCulture);
        }

        private void EnglishButtonOnClick(object sender, MyraEventArgs e)
        {
            SwitchLanguage(EnglishUiCulture, EnglishFormatCulture);
        }

        private void SwitchLanguage(string uiCultureName, string formatCultureName)
        {
            ApplyLanguage(uiCultureName, formatCultureName);
            RefreshDesktopTexts();
            UpdateReport();
        }

        private void RefreshDesktopTexts()
        {
            if (_desktop is null)
            {
                return;
            }

            _lastRefreshedWidgetCount = LanguageRefresher.Refresh(_desktop.Root);
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
            if (_reportLabel is null)
            {
                return;
            }

            string formattedDate = _sampleDate.ToString(SampleDatePattern, CultureInfo.CurrentCulture);
            string formattedNumber = SampleNumber.ToString(SampleNumberPattern, CultureInfo.CurrentCulture);
            _reportLabel.Text = string.Format(
                CultureInfo.InvariantCulture,
                ReportPattern,
                CultureInfo.CurrentUICulture.Name,
                CultureInfo.CurrentCulture.Name,
                formattedDate,
                formattedNumber,
                _lastRefreshedWidgetCount);
        }

        private void AdvanceCapture()
        {
            _drawnFrameCount += 1;
            if (_drawnFrameCount == SpanishCaptureFrame)
            {
                SaveCapture(SpanishCaptureFile);
            }
            else if (_drawnFrameCount == LanguageSwitchFrame)
            {
                SwitchLanguage(EnglishUiCulture, EnglishFormatCulture);
            }
            else if (_drawnFrameCount == EnglishCaptureFrame)
            {
                SaveCapture(EnglishCaptureFile);
                Exit();
            }
        }

        private void SaveCapture(string captureFileName)
        {
            int captureWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int captureHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
            using var captureTarget = new RenderTarget2D(GraphicsDevice, captureWidth, captureHeight);
            GraphicsDevice.SetRenderTarget(captureTarget);
            GraphicsDevice.Clear(_backgroundColor);
            _desktop?.Render();
            GraphicsDevice.SetRenderTarget(null);
            using (FileStream captureStream = File.Create(captureFileName))
            {
                captureTarget.SaveAsPng(captureStream, captureWidth, captureHeight);
            }

            Console.WriteLine(SavedCaptureMessage, Path.GetFullPath(captureFileName));
        }
    }
}
