namespace Torres.Client.Localization
{
    internal sealed class LanguageOption
    {
        internal LanguageOption(string uiCultureName, string formatCultureName, string nameKey)
        {
            UiCultureName = uiCultureName;
            FormatCultureName = formatCultureName;
            NameKey = nameKey;
        }

        internal string UiCultureName { get; }

        internal string FormatCultureName { get; }

        internal string NameKey { get; }
    }
}
