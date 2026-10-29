using System;
using System.Globalization;
using System.Security.Cryptography;

namespace Game.Services
{
    internal static class RoomCodeAlphabet
    {
        private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
        private const int CodeLength = 4;

        internal static string Draw()
        {
            char[] codeCharacters = RandomNumberGenerator.GetItems<char>(Alphabet, CodeLength);
            string roomCode = new string(codeCharacters);

            return roomCode;
        }

        internal static string? Normalize(string? candidateCode)
        {
            string trimmedCode = (candidateCode ?? string.Empty).Trim().ToUpper(CultureInfo.InvariantCulture);
            bool isWellFormed = (trimmedCode.Length == CodeLength) && (trimmedCode.AsSpan().IndexOfAnyExcept(Alphabet) < 0);
            string? normalizedCode = isWellFormed ? trimmedCode : null;

            return normalizedCode;
        }
    }
}
