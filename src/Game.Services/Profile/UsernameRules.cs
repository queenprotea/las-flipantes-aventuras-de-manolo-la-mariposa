using System.Text.RegularExpressions;

namespace Game.Services
{
    internal static class UsernameRules
    {
        private const string UsernameFormat = "^[A-Za-z0-9_]{3,20}$";

        private static readonly Regex _usernamePattern = new Regex(UsernameFormat);

        internal static bool IsValid(string? username)
        {
            bool isValid = (username is not null) && _usernamePattern.IsMatch(username);

            return isValid;
        }
    }
}
