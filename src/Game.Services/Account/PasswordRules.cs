using System.Text.RegularExpressions;

namespace Game.Services
{
    internal static partial class PasswordRules
    {
        private const int PasswordWorkFactor = 11;
        private const string StrengthFormat = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,20}$";

        internal static bool IsValid(string? password)
        {
            bool isValid = (password is not null) && CreateStrengthPattern().IsMatch(password);

            return isValid;
        }

        internal static string Hash(string password)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, PasswordWorkFactor);

            return passwordHash;
        }

        [GeneratedRegex(StrengthFormat, RegexOptions.None, RegexLimits.MatchTimeoutMilliseconds)]
        private static partial Regex CreateStrengthPattern();
    }
}
