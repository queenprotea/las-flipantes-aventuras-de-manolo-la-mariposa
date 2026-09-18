using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Torres.Client.Localization
{
    internal sealed class LanguageService
    {
        private const string PreferenceFileName = "language.txt";
        private const string PreferenceFolderName = "Torres";

        private readonly string _preferencePath;

        internal LanguageService()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                PreferenceFolderName);
            _preferencePath = Path.Combine(folder, PreferenceFileName);
        }

        internal static IReadOnlyList<LanguageOption> Available { get; } = new[]
        {
            new LanguageOption("es", "es-MX", TextKeys.Language.SpanishOption),
            new LanguageOption("en", "en-US", TextKeys.Language.EnglishOption),
        };

        internal LanguageOption Current { get; private set; } = Available[0];

        internal void LoadSavedPreference()
        {
            LanguageOption language = Available[0];
            if (File.Exists(_preferencePath))
            {
                language = Find(File.ReadAllText(_preferencePath).Trim()) ?? Available[0];
            }

            Apply(language);
        }

        internal bool Change(LanguageOption language)
        {
            ArgumentNullException.ThrowIfNull(language);

            if (language.UiCultureName == Current.UiCultureName)
            {
                return false;
            }

            Apply(language);
            Save(language);
            return true;
        }

        internal bool IsCurrent(LanguageOption language)
        {
            ArgumentNullException.ThrowIfNull(language);

            return language.UiCultureName == Current.UiCultureName;
        }

        private static LanguageOption? Find(string uiCultureName)
        {
            foreach (LanguageOption option in Available)
            {
                if (option.UiCultureName == uiCultureName)
                {
                    return option;
                }
            }

            return null;
        }

        private void Apply(LanguageOption language)
        {
            CultureInfo uiCulture = CultureInfo.GetCultureInfo(language.UiCultureName);
            CultureInfo formatCulture = CultureInfo.GetCultureInfo(language.FormatCultureName);

            CultureInfo.CurrentUICulture = uiCulture;
            CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
            CultureInfo.CurrentCulture = formatCulture;
            CultureInfo.DefaultThreadCurrentCulture = formatCulture;
            Current = language;
        }

        private void Save(LanguageOption language)
        {
            string? folder = Path.GetDirectoryName(_preferencePath);
            if (folder is not null)
            {
                Directory.CreateDirectory(folder);
                File.WriteAllText(_preferencePath, language.UiCultureName);
            }
        }
    }
}
