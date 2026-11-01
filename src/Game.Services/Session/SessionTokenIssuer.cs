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
        private const int PayloadFieldCount = 4;
        private const int OwnerKindField = 0;
        private const int OwnerIdField = 1;
        private const int ExpiryField = 2;
        private const int DisplayNameField = 3;
        private const string PlayerKind = "P";
        private const string GuestKind = "G";
        private const string PayloadFormat = "{0}|{1}|{2}|{3}";

        private static readonly TimeSpan _tokenLifetime = TimeSpan.FromMinutes(5);

        private static readonly SessionTokenReading _invalidReading = new SessionTokenReading(SessionTokenStatus.Invalid, 0, string.Empty, false);

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

            string sessionToken = IssueFor(PlayerKind, playerId, username);

            return sessionToken;
        }

        public string IssueForGuest(int guestId, string guestAlias)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(guestId, 0);
            ArgumentException.ThrowIfNullOrWhiteSpace(guestAlias);

            string sessionToken = IssueFor(GuestKind, guestId, guestAlias);

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

        private static int? ReadOwnerId(string ownerKind, string ownerIdText)
        {
            bool isPlayer = string.Equals(ownerKind, PlayerKind, StringComparison.Ordinal);
            bool isGuest = string.Equals(ownerKind, GuestKind, StringComparison.Ordinal);
            NumberStyles ownerIdStyle = isGuest ? NumberStyles.AllowLeadingSign : NumberStyles.None;
            bool isNumber = int.TryParse(ownerIdText, ownerIdStyle, CultureInfo.InvariantCulture, out int parsedOwnerId);
            bool isPlayerId = isPlayer && (parsedOwnerId > 0);
            bool isGuestId = isGuest && (parsedOwnerId < 0);
            int? ownerId = null;
            if (isNumber && (isPlayerId || isGuestId))
            {
                ownerId = parsedOwnerId;
            }

            return ownerId;
        }

        private string IssueFor(string ownerKind, int ownerId, string displayName)
        {
            long expiresAtSeconds = _timeProvider.GetUtcNow().Add(_tokenLifetime).ToUnixTimeSeconds();
            string payload = string.Format(CultureInfo.InvariantCulture, PayloadFormat, ownerKind, ownerId, expiresAtSeconds, displayName);
            string encodedPayload = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(payload));
            string encodedSignature = Base64Url.EncodeToString(Sign(encodedPayload));
            string sessionToken = encodedPayload + PartSeparator + encodedSignature;

            return sessionToken;
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
            string[] payloadFields = payload.Split(FieldSeparator, PayloadFieldCount);
            SessionTokenReading tokenReading = _invalidReading;
            if (payloadFields.Length == PayloadFieldCount)
            {
                tokenReading = ReadFields(payloadFields);
            }

            return tokenReading;
        }

        private SessionTokenReading ReadFields(string[] payloadFields)
        {
            int? ownerId = ReadOwnerId(payloadFields[OwnerKindField], payloadFields[OwnerIdField]);
            bool hasExpiry = long.TryParse(payloadFields[ExpiryField], NumberStyles.None, CultureInfo.InvariantCulture, out long expiresAtSeconds);
            string displayName = payloadFields[DisplayNameField];
            bool isReadable = (ownerId is not null) && hasExpiry && !string.IsNullOrWhiteSpace(displayName);
            SessionTokenReading tokenReading = _invalidReading;
            if (isReadable)
            {
                bool isGuest = string.Equals(payloadFields[OwnerKindField], GuestKind, StringComparison.Ordinal);
                bool hasExpired = _timeProvider.GetUtcNow().ToUnixTimeSeconds() > expiresAtSeconds;
                SessionTokenStatus tokenStatus = hasExpired ? SessionTokenStatus.Expired : SessionTokenStatus.Valid;
                tokenReading = new SessionTokenReading(tokenStatus, ownerId.GetValueOrDefault(), displayName, isGuest);
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
