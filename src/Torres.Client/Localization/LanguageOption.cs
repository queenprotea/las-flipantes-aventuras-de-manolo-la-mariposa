namespace Torres.Client.Localization
{
    internal sealed class LanguageOption
    {
        internal LanguageOption(string uiCultureName, string formatCultureName, string nameKey)
        {
            UICultureName = uiCultureName;
            FormatCultureName = formatCultureName;
            NameKey = nameKey;
        }

        internal string UICultureName { get; }

        internal string FormatCultureName { get; }

        internal string NameKey { get; }
    }
}
