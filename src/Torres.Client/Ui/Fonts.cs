using System;
using System.IO;
using System.Reflection;

using FontStashSharp;

namespace Torres.Client.Ui
{
    /// <summary>
    /// Tipografía del cliente. Los tamaños son los del prototipo
    /// (docs/prototipo/Prototipo-Pantallas-Torres.html), expresados en los mismos píxeles.
    /// El trazo sale de DejaVuSans.ttf, la fuente que el proyecto ya distribuye por su cobertura
    /// de glifos: las familias del prototipo no están en el repositorio.
    /// </summary>
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

        /// <summary>Nombre del juego en la pantalla de arranque: .gamename a 44px.</summary>
        internal static SpriteFontBase Display { get; } = System.GetFont(DisplaySize);

        /// <summary>Nombre del juego en la barra superior: .gamename a 25px.</summary>
        internal static SpriteFontBase Brand { get; } = System.GetFont(BrandSize);

        /// <summary>Título de tarjeta: .card &gt; h3 a 17px.</summary>
        internal static SpriteFontBase Title { get; } = System.GetFont(TitleSize);

        /// <summary>Entrada del menú principal: .mlist button .lb a 17px.</summary>
        internal static SpriteFontBase MenuItem { get; } = System.GetFont(MenuItemSize);

        /// <summary>Texto corrido: body a 14px.</summary>
        internal static SpriteFontBase Body { get; } = System.GetFont(BodySize);

        /// <summary>Botones y campos: .btn y .inp a 13.5px.</summary>
        internal static SpriteFontBase Control { get; } = System.GetFont(ControlSize);

        /// <summary>Ayudas y mensajes: .hint y .msg a 12px.</summary>
        internal static SpriteFontBase Small { get; } = System.GetFont(SmallSize);

        /// <summary>Rótulo de campo y pie de la barra superior: label.f a 11px.</summary>
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
