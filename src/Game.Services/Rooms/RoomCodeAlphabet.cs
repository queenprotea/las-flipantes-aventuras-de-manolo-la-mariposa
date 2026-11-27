using System;
using System.Buffers;
using System.Globalization;
using System.Security.Cryptography;

using Game.Contracts;

namespace Game.Services
{
    internal static class RoomCodeAlphabet
    {
        private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

        private static readonly SearchValues<char> _alphabetCharacters = SearchValues.Create(Alphabet);

        internal static string Draw()
        {
            char[] codeCharacters = RandomNumberGenerator.GetItems<char>(Alphabet, AccountLimits.RoomCodeLength);
            string roomCode = new string(codeCharacters);

            return roomCode;
        }

        internal static string? Normalize(string? candidateCode)
        {
            string trimmedCode = (candidateCode ?? string.Empty).Trim().ToUpper(CultureInfo.InvariantCulture);
            bool isWellFormed = (trimmedCode.Length == AccountLimits.RoomCodeLength) && (trimmedCode.AsSpan().IndexOfAnyExcept(_alphabetCharacters) < 0);
            string? normalizedCode = isWellFormed ? trimmedCode : null;

            return normalizedCode;
        }
    }
}
