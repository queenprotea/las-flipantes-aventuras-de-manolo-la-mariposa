using System;
using System.IO;
using System.Reflection;

using FontStashSharp;

namespace Torres.Client.Ui
{
    internal static class Fonts
    {
        private const string FontResourceName = "DejaVuSans.ttf";

        private const int DisplaySize = 44;
        private const int BrandSize = 25;
        private const int TitleSize = 17;
        private const int MenuItemSize = 17;
        private const int BodySize = 14;
        private const int ControlSize = 13;
        private const int SmallSize = 12;
        private const int LabelSize = 11;

        private static readonly FontSystem System = LoadFontSystem();

        internal static SpriteFontBase Display { get; } = System.GetFont(DisplaySize);

        internal static SpriteFontBase Brand { get; } = System.GetFont(BrandSize);

        internal static SpriteFontBase Title { get; } = System.GetFont(TitleSize);

        internal static SpriteFontBase MenuItem { get; } = System.GetFont(MenuItemSize);

        internal static SpriteFontBase Body { get; } = System.GetFont(BodySize);

        internal static SpriteFontBase Control { get; } = System.GetFont(ControlSize);

        internal static SpriteFontBase Small { get; } = System.GetFont(SmallSize);

        internal static SpriteFontBase Label { get; } = System.GetFont(LabelSize);

        private static FontSystem LoadFontSystem()
        {
            var fontSystem = new FontSystem();
            Assembly assembly = typeof(Fonts).Assembly;
            using (Stream? stream = assembly.GetManifestResourceStream(FontResourceName))
            {
                if (stream is null)
                {
                    throw new InvalidOperationException($"No se encontró el recurso incrustado {FontResourceName}.");
                }

                fontSystem.AddFont(stream);
            }

            return fontSystem;
        }
    }
}
