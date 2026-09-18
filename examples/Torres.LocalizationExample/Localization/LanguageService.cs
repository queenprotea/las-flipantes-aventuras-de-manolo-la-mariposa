using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Torres.LocalizationExample.Localization
{
    /// <summary>
    /// Idioma de la interfaz del cliente. Sustituye a la línea
    /// Thread.CurrentThread.CurrentUICulture = ... del App.xaml.cs de WPF.
    /// La preferencia es del equipo, no de la cuenta (CU-05, POST-2): se guarda en un
    /// archivo local y nunca viaja al servidor.
    /// </summary>
    public sealed class LanguageService
    {
        private const string PreferenceFileName = "language.txt";

        // El nombre de cada idioma se muestra siempre en su propio idioma ("Español", "English"),
        // por eso no vive en los .resx.
        public static readonly IReadOnlyList<LanguageOption> Available = new[]
        {
            new LanguageOption("es", "Español"),
            new LanguageOption("en", "English"),
        };

        private readonly string preferencePath;

        public LanguageService()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Torres");
            preferencePath = Path.Combine(folder, PreferenceFileName);
        }

        public LanguageOption Current { get; private set; } = Available[0];

        public void LoadSavedPreference()
        {
            string code = Available[0].CultureCode;
            if (File.Exists(preferencePath))
            {
                code = File.ReadAllText(preferencePath).Trim();
            }

            Apply(Find(code) ?? Available[0]);
        }

        /// <returns>false si el idioma ya estaba en uso (CU-05, FA02).</returns>
        public bool Change(LanguageOption language)
        {
            if (language.CultureCode == Current.CultureCode)
            {
                return false;
            }

            Apply(language);
            Directory.CreateDirectory(Path.GetDirectoryName(preferencePath)!);
            File.WriteAllText(preferencePath, language.CultureCode);
            return true;
        }

        private void Apply(LanguageOption language)
        {
            var culture = CultureInfo.GetCultureInfo(language.CultureCode);

            // Hilo actual y también los que crean las continuaciones async de WCF.
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Current = language;
        }

        private static LanguageOption? Find(string cultureCode)
        {
            foreach (LanguageOption option in Available)
            {
                if (option.CultureCode == cultureCode)
                {
                    return option;
                }
            }

            return null;
        }
    }

    public sealed record LanguageOption(string CultureCode, string NativeName);
}
