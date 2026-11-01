using System.Globalization;
using System.Resources;

namespace Torres.Client.Localization
{
    internal static class LocalizedText
    {
        private const string ResourceBaseName = "Torres.Client.Resources.Strings";

        private static readonly ResourceManager _resourceManager =
            new ResourceManager(ResourceBaseName, typeof(LocalizedText).Assembly);

        internal static string Get(string key)
        {
            string localizedText = _resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;

            return localizedText;
        }

        internal static string Format(string key, params string[] arguments)
        {
            string textPattern = Get(key);
            string formattedText = string.Format(CultureInfo.CurrentCulture, textPattern, arguments);

            return formattedText;
        }
    }
}
