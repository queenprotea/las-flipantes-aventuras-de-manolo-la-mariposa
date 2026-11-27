using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

using Torres.Client.Threading;

namespace Torres.Client.Localization
{
    internal sealed class LanguagePreference
    {
        private const string PreferenceFileName = "language.txt";
        private const string SpanishUICulture = "es";
        private const string SpanishFormatCulture = "es-MX";
        private const string EnglishUICulture = "en";
        private const string EnglishFormatCulture = "en-US";

        private readonly string _preferencePath;

        private Task<string>? _savedPreferenceReading;
        private Task _preferenceSaving = Task.CompletedTask;
        private bool _wasLanguageChosen;

        internal LanguagePreference()
        {
            string preferenceFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ClientSettings.LocalFolderName);
            _preferencePath = Path.Combine(preferenceFolder, PreferenceFileName);
        }

        internal static LanguageOption Spanish { get; } =
            new LanguageOption(SpanishUICulture, SpanishFormatCulture, TextKeys.Language.SpanishOption);

        internal static LanguageOption English { get; } =
            new LanguageOption(EnglishUICulture, EnglishFormatCulture, TextKeys.Language.EnglishOption);

        internal static IReadOnlyList<LanguageOption> AvailableLanguages { get; } = new LanguageOption[] { Spanish, English };

        internal LanguageOption CurrentLanguage { get; private set; } = Spanish;

        internal bool HasSaveFailed => _preferenceSaving.IsCompleted && !_preferenceSaving.IsCompletedSuccessfully;

        internal void StartLoadingSavedPreference()
        {
            ApplyCulture(Spanish);
            _savedPreferenceReading = new BackgroundCall<string, string>(ReadSavedUICultureName, _preferencePath).StartAsync();
        }

        internal void Update()
        {
            if ((_savedPreferenceReading is null) || !_savedPreferenceReading.IsCompleted)
            {
                return;
            }

            LanguageOption? savedLanguage = _savedPreferenceReading.IsCompletedSuccessfully ? FindByUICultureName(_savedPreferenceReading.Result) : null;
            _savedPreferenceReading = null;
            if ((savedLanguage is not null) && !_wasLanguageChosen)
            {
                ApplyCulture(savedLanguage);
            }
        }

        internal bool Change(LanguageOption language)
        {
            if (language.UICultureName == CurrentLanguage.UICultureName)
            {
                return false;
            }

            _wasLanguageChosen = true;
            ApplyCulture(language);
            _preferenceSaving = new BackgroundWork<LanguageOption>(SavePreference, language).StartAsync();

            return true;
        }

        internal bool IsCurrent(LanguageOption language)
        {
            bool isCurrentLanguage = language.UICultureName == CurrentLanguage.UICultureName;

            return isCurrentLanguage;
        }

        private static string ReadSavedUICultureName(string preferencePath)
        {
            string savedUICultureName = File.Exists(preferencePath) ? File.ReadAllText(preferencePath).Trim() : string.Empty;

            return savedUICultureName;
        }

        private static LanguageOption? FindByUICultureName(string uiCultureName)
        {
            foreach (LanguageOption availableLanguage in AvailableLanguages)
            {
                if (availableLanguage.UICultureName == uiCultureName)
                {
                    return availableLanguage;
                }
            }

            return null;
        }

        private void ApplyCulture(LanguageOption language)
        {
            CultureInfo uiCulture = CultureInfo.GetCultureInfo(language.UICultureName);
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
            File.WriteAllText(_preferencePath, language.UICultureName);
        }
    }
}
