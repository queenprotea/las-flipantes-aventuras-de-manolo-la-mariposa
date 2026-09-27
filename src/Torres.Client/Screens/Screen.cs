using FontStashSharp.RichText;

using Microsoft.Xna.Framework;

using Myra.Graphics2D;
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
        
        internal object[]? HeaderArguments { get; private protected set; }

        internal Widget Root => _root ??= Build();

        internal bool IsBuilt => _root is not null;

        internal virtual void Update(GameTime gameTime)
        {
        }

        internal virtual void Open()
        {
        }

        protected abstract Widget Build();

        protected static VerticalStackPanel Page()
        {
            return new VerticalStackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
        }

        protected static HorizontalStackPanel Columns()
        {
            return new HorizontalStackPanel
            {
                Spacing = Metrics.ColumnSpacing,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
        }

        protected static VerticalStackPanel Card(int width)
        {
            return new VerticalStackPanel
            {
                Spacing = Metrics.FieldSpacing,
                Width = width,
                Padding = Metrics.CardPadding,
                Background = Theme.SurfaceBrush,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Top,
            };
        }

        protected static VerticalStackPanel CardHeader(string titleKey, string hintKey)
        {
            var header = new VerticalStackPanel { Spacing = Metrics.CardHeaderSpacing };
            header.Widgets.Add(Title(titleKey));
            header.Widgets.Add(Hint(hintKey));
            return header;
        }

        protected static LocalizedLabel Title(string textKey)
        {
            return new LocalizedLabel(textKey)
            {
                Font = Fonts.Title,
                TextColor = Theme.Ink,
                Wrap = true,
            };
        }

        protected static LocalizedLabel Hint(string textKey)
        {
            return new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                Wrap = true,
            };
        }

        protected static LocalizedLabel Paragraph(string textKey)
        {
            return new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.SoftInk,
                Wrap = true,
            };
        }

        protected static LocalizedLabel Error(string textKey)
        {
            return new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.BlushInk,
                Wrap = true,
                Visible = false,
            };
        }

        protected static LocalizedLabel Success(string textKey)
        {
            return new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.MintInk,
                Wrap = true,
            };
        }

        protected static LocalizedButton PrimaryButton(string textKey)
        {
            LocalizedButton button = BaseButton(textKey, Theme.MintInk);
            button.Background = Theme.MintBrush;
            button.OverBackground = Theme.MintAltBrush;
            button.PressedBackground = Theme.MintAltBrush;
            button.Border = Theme.MintLineBrush;
            return button;
        }

        protected static LocalizedButton SecondaryButton(string textKey)
        {
            LocalizedButton button = BaseButton(textKey, Theme.Ink);
            button.Background = Theme.SurfaceBrush;
            button.OverBackground = Theme.SurfaceSunkenBrush;
            button.PressedBackground = Theme.SurfaceSunkenBrush;
            button.Border = Theme.StrongLineBrush;
            return button;
        }

        protected static LocalizedButton SmallSecondaryButton(string textKey)
        {
            LocalizedButton button = SecondaryButton(textKey);
            button.LabelFont = Fonts.Small;
            button.Padding = Metrics.SmallButtonPadding;
            return button;
        }

        protected static LocalizedButton DestructiveButton(string textKey)
        {
            LocalizedButton button = BaseButton(textKey, Theme.BlushInk);
            button.Background = Theme.BlushTintBrush;
            button.OverBackground = Theme.BlushTintBrush;
            button.PressedBackground = Theme.BlushTintBrush;
            button.Border = Theme.BlushBrush;
            return button;
        }

        protected static LocalizedButton LinkButton(string textKey)
        {
            var button = new LocalizedButton(textKey)
            {
                LabelColor = Theme.LavenderInk,
                LabelFont = Fonts.Small,
                Padding = Metrics.LinkButtonPadding,
                Background = null,
                OverBackground = null,
                PressedBackground = null,
                Border = null,
                BorderThickness = new Thickness(0),
            };
            return button;
        }

        protected static HorizontalStackPanel Row()
        {
            return new HorizontalStackPanel { Spacing = Metrics.ButtonSpacing };
        }

        protected static VerticalStackPanel WrappedRow()
        {
            return new VerticalStackPanel
            {
                Spacing = Metrics.ButtonSpacing,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
        }

        protected static HorizontalStackPanel BackBar(LocalizedButton backButton)
        {
            HorizontalStackPanel bar = Row();
            bar.Margin = new Thickness(0, Metrics.ColumnSpacing, 0, 0);
            bar.VerticalAlignment = VerticalAlignment.Bottom;
            bar.Widgets.Add(backButton);
            return bar;
        }

        protected static Panel Ambience()
        {
            return new Panel
            {
                Background = Theme.MintTintBrush,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
        }

        protected static Panel Avatar()
        {
            return new Panel
            {
                Width = Sizes.LargeAvatarSize,
                Height = Sizes.LargeAvatarSize,
                Background = Theme.MintAltBrush,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
        }

        internal static Panel Divider()
        {
            return new Panel
            {
                Height = Sizes.BorderSize,
                Background = Theme.LineBrush,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
        }

        private static LocalizedButton BaseButton(string textKey, Color labelColor)
        {
            return new LocalizedButton(textKey)
            {
                LabelColor = labelColor,
                LabelFont = Fonts.Control,
                Padding = Metrics.ButtonPadding,
                BorderThickness = Sizes.Border,
            };
        }
        
        protected static VerticalStackPanel EmptyState(string titleKey, string hintKey)
        {
            var emptyState = new VerticalStackPanel
            {
                Spacing = Metrics.FieldSpacing,
                Width = Sizes.EmptyStateWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            LocalizedLabel title = Title(titleKey);
            title.TextAlign = TextHorizontalAlignment.Center;
            emptyState.Widgets.Add(title);

            LocalizedLabel hint = Hint(hintKey);
            hint.TextAlign = TextHorizontalAlignment.Center;
            emptyState.Widgets.Add(hint);

            return emptyState;
        }
    }
}
