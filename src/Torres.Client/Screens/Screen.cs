using Microsoft.Xna.Framework;

using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal abstract class Screen
    {
        private Widget? _root;

        protected Screen(string headerKey, bool showsTopBar)
        {
            HeaderKey = headerKey;
            ShowsTopBar = showsTopBar;
        }

        internal string HeaderKey { get; }

        internal bool ShowsTopBar { get; }

        internal ScreenId? RequestedScreen { get; set; }

        internal Widget Root => _root ??= Build();

        internal bool IsBuilt => _root is not null;

        internal virtual void Update(GameTime gameTime)
        {
        }

        protected abstract Widget Build();

        protected static VerticalStackPanel Card()
        {
            return new VerticalStackPanel
            {
                Spacing = Theme.Spacing,
                Width = Theme.CardWidth,
                Padding = Theme.CardPadding,
                Background = Theme.CardBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Myra.Graphics2D.Thickness(0, 40, 0, 0),
            };
        }

        protected static LocalizedLabel Title(string textKey)
        {
            return new LocalizedLabel(textKey) { TextColor = Theme.Text };
        }

        protected static LocalizedLabel Hint(string textKey)
        {
            return new LocalizedLabel(textKey) { TextColor = Theme.MutedText, Wrap = true };
        }

        protected static LocalizedLabel Error(string textKey)
        {
            return new LocalizedLabel(textKey) { TextColor = Theme.Danger, Wrap = true, Visible = false };
        }

        protected static LocalizedButton PrimaryButton(string textKey)
        {
            return new LocalizedButton(textKey) { LabelColor = Theme.Accent };
        }

        protected static LocalizedButton SecondaryButton(string textKey)
        {
            return new LocalizedButton(textKey) { LabelColor = Theme.Text };
        }

        protected static LocalizedButton DestructiveButton(string textKey)
        {
            return new LocalizedButton(textKey) { LabelColor = Theme.Danger };
        }

        protected static HorizontalStackPanel Row()
        {
            return new HorizontalStackPanel { Spacing = 8 };
        }
    }
}
