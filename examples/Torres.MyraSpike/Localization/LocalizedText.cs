using System.Globalization;
using System.Resources;

namespace Torres.MyraSpike.Localization
{
    internal static class LocalizedText
    {
        private static readonly ResourceManager Manager =
            new ResourceManager("Torres.MyraSpike.Resources.Strings", typeof(LocalizedText).Assembly);

        internal static string Get(string key)
        {
            return Manager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
        }
    }
}
