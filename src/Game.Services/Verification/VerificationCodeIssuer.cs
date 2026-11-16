using System.Globalization;
using System.Security.Cryptography;

namespace Game.Services
{
    internal static class VerificationCodeIssuer
    {
        private const int CodeUpperBound = 1000000;
        private const string CodeFormat = "D6";

        internal static string Issue()
        {
            int codeNumber = RandomNumberGenerator.GetInt32(CodeUpperBound);
            string code = codeNumber.ToString(CodeFormat, CultureInfo.InvariantCulture);

            return code;
        }
    }
}
