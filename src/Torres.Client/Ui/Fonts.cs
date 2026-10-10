using System;
using System.IO;
using System.Reflection;

using FontStashSharp;

namespace Torres.Client.Ui
{
    internal static class Fonts
    {
        private const string FontResourceName = "DejaVuSans.ttf";
        private const string MissingFontMessage = "The embedded resource DejaVuSans.ttf was not found.";

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

        private static FontSystem? _fontSystem;

        internal static SpriteFontBase Display => GetFontSystem().GetFont(DisplaySize);

        internal static SpriteFontBase Brand => GetFontSystem().GetFont(BrandSize);

        internal static SpriteFontBase Winner => GetFontSystem().GetFont(WinnerSize);

        internal static SpriteFontBase Headline => GetFontSystem().GetFont(HeadlineSize);

        internal static SpriteFontBase Notice => GetFontSystem().GetFont(NoticeSize);

        internal static SpriteFontBase Heading => GetFontSystem().GetFont(HeadingSize);

        internal static SpriteFontBase Title => GetFontSystem().GetFont(TitleSize);

        internal static SpriteFontBase MenuItem => GetFontSystem().GetFont(MenuItemSize);

        internal static SpriteFontBase Body => GetFontSystem().GetFont(BodySize);

        internal static SpriteFontBase Control => GetFontSystem().GetFont(ControlSize);

        internal static SpriteFontBase Small => GetFontSystem().GetFont(SmallSize);

        internal static SpriteFontBase Label => GetFontSystem().GetFont(LabelSize);

        internal static SpriteFontBase Clock => GetFontSystem().GetFont(ClockSize);

        internal static SpriteFontBase LargeClock => GetFontSystem().GetFont(LargeClockSize);

        internal static SpriteFontBase ActionPoints => GetFontSystem().GetFont(ActionPointsSize);

        internal static SpriteFontBase RoomCode => GetFontSystem().GetFont(RoomCodeSize);

        private static FontSystem GetFontSystem()
        {
            _fontSystem ??= LoadFontSystem();

            return _fontSystem;
        }

        private static FontSystem LoadFontSystem()
        {
            var fontSystem = new FontSystem();
            Assembly clientAssembly = typeof(Fonts).Assembly;
            using (Stream? fontStream = clientAssembly.GetManifestResourceStream(FontResourceName))
            {
                if (fontStream is null)
                {
                    throw new InvalidOperationException(MissingFontMessage);
                }

                fontSystem.AddFont(fontStream);
            }

            return fontSystem;
        }
    }
}
