using System;
using System.Buffers.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Game.Services
{
    public sealed class SessionTokenIssuer
    {
        private const int ShortestSigningKeyBytes = 32;
        private const char PartSeparator = '.';
        private const char FieldSeparator = '|';
        private const int TokenPartCount = 2;
        private const int PayloadFieldCount = 3;
        private const int PlayerIdField = 0;
        private const int UsernameField = 1;
        private const int ExpiryField = 2;
        private const string PayloadFormat = "{0}|{1}|{2}";

        private static readonly TimeSpan _tokenLifetime = TimeSpan.FromMinutes(5);

        private static readonly SessionTokenReading _invalidReading = new SessionTokenReading(SessionTokenStatus.Invalid, 0, string.Empty);

        private readonly byte[] _signingKey;
        private readonly TimeProvider _timeProvider;

        public SessionTokenIssuer(byte[] signingKey, TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(signingKey);
            ArgumentNullException.ThrowIfNull(timeProvider);
            ArgumentOutOfRangeException.ThrowIfLessThan(signingKey.Length, ShortestSigningKeyBytes, nameof(signingKey));

            _signingKey = signingKey;
            _timeProvider = timeProvider;
        }

        public string Issue(int playerId, string username)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(playerId);
            ArgumentException.ThrowIfNullOrWhiteSpace(username);

            long expiresAtSeconds = _timeProvider.GetUtcNow().Add(_tokenLifetime).ToUnixTimeSeconds();
            string payload = string.Format(CultureInfo.InvariantCulture, PayloadFormat, playerId, username, expiresAtSeconds);
            string encodedPayload = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(payload));
            string encodedSignature = Base64Url.EncodeToString(Sign(encodedPayload));
            string sessionToken = encodedPayload + PartSeparator + encodedSignature;

            return sessionToken;
        }

        internal SessionTokenReading Read(string sessionToken)
        {
            ArgumentNullException.ThrowIfNull(sessionToken);

            string[] tokenParts = sessionToken.Split(PartSeparator);
            bool isSigned = (tokenParts.Length == TokenPartCount) && HasValidSignature(tokenParts[0], tokenParts[1]);
            SessionTokenReading tokenReading = isSigned ? ReadPayload(tokenParts[0]) : _invalidReading;

            return tokenReading;
        }

        private bool HasValidSignature(string encodedPayload, string encodedSignature)
        {
            byte[] expectedSignature = Sign(encodedPayload);
            bool isValidSignature = Base64Url.IsValid(encodedSignature)
                && CryptographicOperations.FixedTimeEquals(expectedSignature, Base64Url.DecodeFromChars(encodedSignature));

            return isValidSignature;
        }

        private SessionTokenReading ReadPayload(string encodedPayload)
        {
            string payload = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(encodedPayload));
            string[] payloadFields = payload.Split(FieldSeparator);
            SessionTokenReading tokenReading = _invalidReading;
            if ((payloadFields.Length == PayloadFieldCount)
                && int.TryParse(payloadFields[PlayerIdField], NumberStyles.None, CultureInfo.InvariantCulture, out int playerId)
                && long.TryParse(payloadFields[ExpiryField], NumberStyles.None, CultureInfo.InvariantCulture, out long expiresAtSeconds))
            {
                bool hasExpired = _timeProvider.GetUtcNow().ToUnixTimeSeconds() > expiresAtSeconds;
                SessionTokenStatus tokenStatus = hasExpired ? SessionTokenStatus.Expired : SessionTokenStatus.Valid;
                tokenReading = new SessionTokenReading(tokenStatus, playerId, payloadFields[UsernameField]);
            }

            return tokenReading;
        }

        private byte[] Sign(string encodedPayload)
        {
            byte[] signature = HMACSHA256.HashData(_signingKey, Encoding.UTF8.GetBytes(encodedPayload));

            return signature;
        }
    }
}
