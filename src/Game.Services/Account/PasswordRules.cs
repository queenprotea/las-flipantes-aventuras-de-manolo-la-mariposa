using System.Text.RegularExpressions;

namespace Game.Services
{
    internal static class PasswordRules
    {
        private const int PasswordWorkFactor = 11;
        private const string PasswordFormat = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,20}$";

        private static readonly Regex _passwordPattern = new Regex(PasswordFormat);

        internal static bool IsValid(string? password)
        {
            bool isValid = (password is not null) && _passwordPattern.IsMatch(password);

            return isValid;
        }

        internal static string Hash(string password)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, PasswordWorkFactor);

            return passwordHash;
        }
    }
}
