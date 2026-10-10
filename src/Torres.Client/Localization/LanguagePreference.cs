using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Torres.Client.Localization
{
    internal sealed class LanguagePreference
    {
        private const string PreferenceFileName = "language.txt";
        private const string PreferenceFolderName = "Torres";
        private const string SpanishUiCulture = "es";
        private const string SpanishFormatCulture = "es-MX";
        private const string EnglishUiCulture = "en";
        private const string EnglishFormatCulture = "en-US";

        private readonly string _preferencePath;

        internal LanguagePreference()
        {
            string preferenceFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                PreferenceFolderName);
            _preferencePath = Path.Combine(preferenceFolder, PreferenceFileName);
        }

        internal static LanguageOption Spanish { get; } =
            new LanguageOption(SpanishUiCulture, SpanishFormatCulture, TextKeys.Language.SpanishOption);

        internal static LanguageOption English { get; } =
            new LanguageOption(EnglishUiCulture, EnglishFormatCulture, TextKeys.Language.EnglishOption);

        internal static IReadOnlyList<LanguageOption> AvailableLanguages { get; } = new[] { Spanish, English };

        internal LanguageOption CurrentLanguage { get; private set; } = Spanish;

        internal void LoadSavedPreference()
        {
            LanguageOption savedLanguage = Spanish;
            if (File.Exists(_preferencePath))
            {
                string savedUiCultureName = File.ReadAllText(_preferencePath).Trim();
                savedLanguage = FindByUiCultureName(savedUiCultureName) ?? Spanish;
            }

            ApplyCulture(savedLanguage);
        }

        internal bool Change(LanguageOption language)
        {
            ArgumentNullException.ThrowIfNull(language);

            if (language.UiCultureName == CurrentLanguage.UiCultureName)
            {
                return false;
            }

            ApplyCulture(language);
            SavePreference(language);

            return true;
        }

        internal bool IsCurrent(LanguageOption language)
        {
            ArgumentNullException.ThrowIfNull(language);

            bool isCurrentLanguage = language.UiCultureName == CurrentLanguage.UiCultureName;

            return isCurrentLanguage;
        }

        private static LanguageOption? FindByUiCultureName(string uiCultureName)
        {
            foreach (LanguageOption availableLanguage in AvailableLanguages)
            {
                if (availableLanguage.UiCultureName == uiCultureName)
                {
                    return availableLanguage;
                }
            }

            return null;
        }

        private void ApplyCulture(LanguageOption language)
        {
            CultureInfo uiCulture = CultureInfo.GetCultureInfo(language.UiCultureName);
            CultureInfo formatCulture = CultureInfo.GetCultureInfo(language.FormatCultureName);

            CultureInfo.CurrentUICulture = uiCulture;
            CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
            CultureInfo.CurrentCulture = formatCulture;
            CultureInfo.DefaultThreadCurrentCulture = formatCulture;
            CurrentLanguage = language;
        }

        private void SavePreference(LanguageOption language)
        {
            string? preferenceFolder = Path.GetDirectoryName(_preferencePath);
            if (preferenceFolder is null)
            {
                return;
            }

            Directory.CreateDirectory(preferenceFolder);
            File.WriteAllText(_preferencePath, language.UiCultureName);
        }
    }
}
