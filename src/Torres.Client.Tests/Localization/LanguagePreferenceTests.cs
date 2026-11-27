using Torres.Client.Localization;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class LanguagePreferenceTests
    {
        private readonly LanguagePreference _languagePreference = new LanguagePreference();

        [Fact]
        public void Test_Change_ToTheLanguageInUse_ReturnsFalse()
        {
            bool hasChanged = _languagePreference.Change(LanguagePreference.Spanish);

            Assert.False(hasChanged);
        }
    }
}
