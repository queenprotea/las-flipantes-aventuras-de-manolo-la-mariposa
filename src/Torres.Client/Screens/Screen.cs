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

        internal Widget Root => _root ??= Build();

        internal bool IsBuilt => _root is not null;

        internal virtual void Update(GameTime gameTime)
        {
        }

        protected abstract Widget Build();

        /// <summary>Cuerpo de una pantalla: el contenido y, debajo, su retorno.</summary>
        protected static VerticalStackPanel Page()
        {
            return new VerticalStackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
        }

        /// <summary>.row: columnas separadas 16px.</summary>
        protected static HorizontalStackPanel Columns()
        {
            return new HorizontalStackPanel
            {
                Spacing = Theme.ColumnSpacing,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
        }

        /// <summary>.card: fondo blanco, borde de 1px y relleno de 18px.</summary>
        protected static VerticalStackPanel Card(int width)
        {
            return new VerticalStackPanel
            {
                Spacing = Theme.FieldSpacing,
                Width = width,
                Padding = Theme.CardPadding,
                Background = Theme.SurfaceBrush,
                Border = Theme.LineBrush,
                BorderThickness = Theme.Border,
                VerticalAlignment = VerticalAlignment.Top,
            };
        }

        /// <summary>Encabezado de tarjeta: el h3 y su .hint, pegados como en el prototipo.</summary>
        protected static VerticalStackPanel CardHeader(string titleKey, string hintKey)
        {
            var header = new VerticalStackPanel { Spacing = Theme.CardHeaderSpacing };
            header.Widgets.Add(Title(titleKey));
            header.Widgets.Add(Hint(hintKey));
            return header;
        }

        /// <summary>.card &gt; h3.</summary>
        protected static LocalizedLabel Title(string textKey)
        {
            return new LocalizedLabel(textKey)
            {
                Font = Fonts.Title,
                TextColor = Theme.Ink,
                Wrap = true,
            };
        }

        /// <summary>.hint.</summary>
        protected static LocalizedLabel Hint(string textKey)
        {
            return new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                Wrap = true,
            };
        }

        /// <summary>Párrafo explicativo dentro de una tarjeta.</summary>
        protected static LocalizedLabel Paragraph(string textKey)
        {
            return new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.SoftInk,
                Wrap = true,
            };
        }

        /// <summary>.msg.bad.</summary>
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

        /// <summary>.msg.good.</summary>
        protected static LocalizedLabel Success(string textKey)
        {
            return new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.MintInk,
                Wrap = true,
            };
        }

        /// <summary>.btn.pri.</summary>
        protected static LocalizedButton PrimaryButton(string textKey)
        {
            LocalizedButton button = BaseButton(textKey, Theme.MintInk);
            button.Background = Theme.MintBrush;
            button.OverBackground = Theme.MintAltBrush;
            button.PressedBackground = Theme.MintAltBrush;
            button.Border = Theme.MintLineBrush;
            return button;
        }

        /// <summary>.btn.</summary>
        protected static LocalizedButton SecondaryButton(string textKey)
        {
            LocalizedButton button = BaseButton(textKey, Theme.Ink);
            button.Background = Theme.SurfaceBrush;
            button.OverBackground = Theme.SurfaceSunkenBrush;
            button.PressedBackground = Theme.SurfaceSunkenBrush;
            button.Border = Theme.StrongLineBrush;
            return button;
        }

        /// <summary>.btn.sm: la variante compacta del botón secundario.</summary>
        protected static LocalizedButton SmallSecondaryButton(string textKey)
        {
            LocalizedButton button = SecondaryButton(textKey);
            button.LabelFont = Fonts.Small;
            button.Padding = Theme.SmallButtonPadding;
            return button;
        }

        /// <summary>.btn.dgr.</summary>
        protected static LocalizedButton DestructiveButton(string textKey)
        {
            LocalizedButton button = BaseButton(textKey, Theme.BlushInk);
            button.Background = Theme.BlushTintBrush;
            button.OverBackground = Theme.BlushTintBrush;
            button.PressedBackground = Theme.BlushTintBrush;
            button.Border = Theme.BlushBrush;
            return button;
        }

        /// <summary>Botón sin caja: el enlace en color lavanda del pie de PT-03.</summary>
        protected static LocalizedButton LinkButton(string textKey)
        {
            var button = new LocalizedButton(textKey)
            {
                LabelColor = Theme.LavenderInk,
                LabelFont = Fonts.Small,
                Padding = Theme.LinkButtonPadding,
                Background = null,
                OverBackground = null,
                PressedBackground = null,
                Border = null,
                BorderThickness = new Thickness(0),
            };
            return button;
        }

        /// <summary>.btns: botones separados 9px.</summary>
        protected static HorizontalStackPanel Row()
        {
            return new HorizontalStackPanel { Spacing = Theme.ButtonSpacing };
        }

        /// <summary>
        /// .btns cuando sus botones no caben en el ancho de la tarjeta: flex-wrap los baja
        /// uno por fila, y eso es lo que se reproduce aquí.
        /// </summary>
        protected static VerticalStackPanel WrappedRow()
        {
            return new VerticalStackPanel
            {
                Spacing = Theme.ButtonSpacing,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
        }

        /// <summary>backbar(): el retorno de la pantalla, al pie y fuera de la tarjeta.</summary>
        protected static HorizontalStackPanel BackBar(LocalizedButton backButton)
        {
            HorizontalStackPanel bar = Row();
            bar.Margin = new Thickness(0, Theme.ColumnSpacing, 0, 0);
            bar.VerticalAlignment = VerticalAlignment.Bottom;
            bar.Widgets.Add(backButton);
            return bar;
        }

        /// <summary>.ambience: el recuadro de ambientación que acompaña a las tarjetas.</summary>
        protected static Panel Ambience()
        {
            return new Panel
            {
                Background = Theme.MintTintBrush,
                Border = Theme.LineBrush,
                BorderThickness = Theme.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
        }

        /// <summary>.av.lg: el recuadro del avatar, de 92px y fondo mentolado.</summary>
        protected static Panel Avatar()
        {
            return new Panel
            {
                Width = Theme.LargeAvatarSize,
                Height = Theme.LargeAvatarSize,
                Background = Theme.MintAltBrush,
                Border = Theme.LineBrush,
                BorderThickness = Theme.Border,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
        }

        /// <summary>Línea de 1px: el hr y los bordes inferiores del prototipo.</summary>
        internal static Panel Divider()
        {
            return new Panel
            {
                Height = Theme.BorderSize,
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
                Padding = Theme.ButtonPadding,
                BorderThickness = Theme.Border,
            };
        }
    }
}
