using System.Globalization;
using System.Resources;

namespace Torres.MyraSpike.Localization
{
    internal static class LocalizedText
    {
        private const string ResourceBaseName = "Torres.MyraSpike.Resources.Strings";

        private static readonly ResourceManager _resourceManager =
            new ResourceManager(ResourceBaseName, typeof(LocalizedText).Assembly);

        internal static string Get(string key)
        {
            string localizedText = _resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;

            return localizedText;
        }
    }
}
