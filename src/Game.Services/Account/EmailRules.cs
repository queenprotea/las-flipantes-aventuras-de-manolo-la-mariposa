using System.Text.RegularExpressions;

using Game.Contracts;

namespace Game.Services
{
    internal static class EmailRules
    {
        private const string EmailFormat = @"^[^@\s]+@[^@\s.]+(\.[^@\s.]+)*\.[A-Za-z]{2,}$";

        private static readonly Regex _emailPattern = new Regex(EmailFormat);

        internal static bool IsValid(string? email)
        {
            bool isValid = (email is not null) && (email.Length <= AccountLimits.LongestEmail) && _emailPattern.IsMatch(email);

            return isValid;
        }
    }
}
