using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Torres.LocalizationExample.Localization
{
    public sealed class LanguageService
    {
        private const string PreferenceFileName = "language.txt";
        private const string PreferenceFolderName = "Torres";
        private const string SpanishCultureCode = "es";
        private const string SpanishNativeName = "Español";
        private const string EnglishCultureCode = "en";
        private const string EnglishNativeName = "English";

        private readonly string _preferenceFolder;
        private readonly string _preferencePath;

        public LanguageService()
        {
            string applicationDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _preferenceFolder = Path.Combine(applicationDataFolder, PreferenceFolderName);
            _preferencePath = Path.Combine(_preferenceFolder, PreferenceFileName);
        }

        public static IReadOnlyList<LanguageOption> AvailableLanguages { get; } = new[]
        {
            new LanguageOption(SpanishCultureCode, SpanishNativeName),
            new LanguageOption(EnglishCultureCode, EnglishNativeName),
        };

        public LanguageOption CurrentLanguage { get; private set; } = AvailableLanguages[0];

        public void LoadSavedPreference()
        {
            string savedCultureCode = AvailableLanguages[0].CultureCode;
            if (File.Exists(_preferencePath))
            {
                savedCultureCode = File.ReadAllText(_preferencePath).Trim();
            }

            LanguageOption savedLanguage = FindByCultureCode(savedCultureCode) ?? AvailableLanguages[0];
            ApplyCulture(savedLanguage);
        }

        public bool Change(LanguageOption language)
        {
            ArgumentNullException.ThrowIfNull(language);

            if (language.CultureCode == CurrentLanguage.CultureCode)
            {
                return false;
            }

            ApplyCulture(language);
            Directory.CreateDirectory(_preferenceFolder);
            File.WriteAllText(_preferencePath, language.CultureCode);

            return true;
        }

        private void ApplyCulture(LanguageOption language)
        {
            CultureInfo uiCulture = CultureInfo.GetCultureInfo(language.CultureCode);
            CultureInfo.CurrentUICulture = uiCulture;
            CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
            CurrentLanguage = language;
        }

        private static LanguageOption? FindByCultureCode(string cultureCode)
        {
            foreach (LanguageOption availableLanguage in AvailableLanguages)
            {
                if (availableLanguage.CultureCode == cultureCode)
                {
                    return availableLanguage;
                }
            }

            return null;
        }
    }

    public sealed record LanguageOption(string CultureCode, string NativeName);
}
