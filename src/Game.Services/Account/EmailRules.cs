using System.Text.RegularExpressions;

using Game.Contracts;

namespace Game.Services
{
    internal static partial class EmailRules
    {
        private const string EmailFormat = @"^[^@\s]+@[^@\s.]+(\.[^@\s.]+)*\.[A-Za-z]{2,}$";

        internal static bool IsValid(string? email)
        {
            bool isValid = (email is not null) && (email.Length <= AccountLimits.LongestEmail) && CreateEmailPattern().IsMatch(email);

            return isValid;
        }

        [GeneratedRegex(EmailFormat, RegexOptions.None, RegexLimits.MatchTimeoutMilliseconds)]
        private static partial Regex CreateEmailPattern();
    }
}
