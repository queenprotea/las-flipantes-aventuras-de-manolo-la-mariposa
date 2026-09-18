using System.Globalization;
using System.Resources;

namespace Torres.Client.Localization
{
    internal static class LocalizedText
    {
        private static readonly ResourceManager Manager =
            new ResourceManager("Torres.Client.Resources.Strings", typeof(LocalizedText).Assembly);

        internal static string Get(string key)
        {
            return Manager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
        }

        internal static string Format(string key, params object[] arguments)
        {
            return string.Format(CultureInfo.CurrentCulture, Get(key), arguments);
        }
    }
}
