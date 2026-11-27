using System.Text.RegularExpressions;

namespace Game.Services
{
    internal static partial class UsernameRules
    {
        private const string UsernameFormat = "^[A-Za-z0-9_]{3,20}$";

        internal static bool IsValid(string? username)
        {
            bool isValid = (username is not null) && CreateUsernamePattern().IsMatch(username);

            return isValid;
        }

        [GeneratedRegex(UsernameFormat, RegexOptions.None, RegexLimits.MatchTimeoutMilliseconds)]
        private static partial Regex CreateUsernamePattern();
    }
}
