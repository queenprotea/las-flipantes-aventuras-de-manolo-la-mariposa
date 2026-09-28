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
        private const int WinnerSize = 26;
        private const int HeadlineSize = 24;
        private const int NoticeSize = 23;
        private const int LargeClockSize = 40;
        private const int HeadingSize = 22;
        private const int TitleSize = 17;
        private const int MenuItemSize = 17;
        private const int BodySize = 14;
        private const int ControlSize = 13;
        private const int SmallSize = 12;
        private const int LabelSize = 11;
        private const int ClockSize = 34;
        private const int ActionPointsSize = 40;
        private const int RoomCodeSize = 23;

        private static readonly FontSystem _fontSystem = LoadFontSystem();

        internal static SpriteFontBase Display { get; } = _fontSystem.GetFont(DisplaySize);

        internal static SpriteFontBase Brand { get; } = _fontSystem.GetFont(BrandSize);

        internal static SpriteFontBase Winner { get; } = _fontSystem.GetFont(WinnerSize);

        internal static SpriteFontBase Headline { get; } = _fontSystem.GetFont(HeadlineSize);

        internal static SpriteFontBase Notice { get; } = _fontSystem.GetFont(NoticeSize);

        internal static SpriteFontBase Heading { get; } = _fontSystem.GetFont(HeadingSize);

        internal static SpriteFontBase Title { get; } = _fontSystem.GetFont(TitleSize);

        internal static SpriteFontBase MenuItem { get; } = _fontSystem.GetFont(MenuItemSize);

        internal static SpriteFontBase Body { get; } = _fontSystem.GetFont(BodySize);

        internal static SpriteFontBase Control { get; } = _fontSystem.GetFont(ControlSize);

        internal static SpriteFontBase Small { get; } = _fontSystem.GetFont(SmallSize);

        internal static SpriteFontBase Label { get; } = _fontSystem.GetFont(LabelSize);

        internal static SpriteFontBase Clock { get; } = _fontSystem.GetFont(ClockSize);

        internal static SpriteFontBase LargeClock { get; } = _fontSystem.GetFont(LargeClockSize);

        internal static SpriteFontBase ActionPoints { get; } = _fontSystem.GetFont(ActionPointsSize);

        internal static SpriteFontBase RoomCode { get; } = _fontSystem.GetFont(RoomCodeSize);

        private static FontSystem LoadFontSystem()
        {
            var fontSystem = new FontSystem();
            Assembly assembly = typeof(Fonts).Assembly;
            using (Stream? stream = assembly.GetManifestResourceStream(FontResourceName))
            {
                if (stream is null)
                {
                    throw new InvalidOperationException($"The embedded resource {FontResourceName} was not found.");
                }

                fontSystem.AddFont(stream);
            }

            return fontSystem;
        }
    }
}
