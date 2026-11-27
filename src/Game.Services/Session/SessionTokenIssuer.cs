using System;
using System.Buffers.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Game.Services
{
    public sealed class SessionTokenIssuer
    {
        public const int ShortestSigningKeyBytes = 32;
        private const char PartSeparator = '.';
        private const char FieldSeparator = '|';
        private const int TokenPartCount = 2;
        private const int PayloadFieldCount = 4;
        private const int OwnerKindField = 0;
        private const int OwnerIdField = 1;
        private const int ExpiryField = 2;
        private const int DisplayNameField = 3;
        private const int SessionOwnerFieldsStart = 0;
        private const int ReconnectionFieldCount = 6;
        private const int ReconnectionMatchIdField = 1;
        private const int ReconnectionOwnerFieldsStart = 2;
        private const string PlayerOwnerKind = "P";
        private const string GuestOwnerKind = "G";
        private const string ReconnectionKind = "R";
        private const string PayloadFormat = "{0}|{1}|{2}|{3}";
        private const string ReconnectionPayloadFormat = "{0}|{1}|{2}|{3}|{4}|{5}";

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

            string sessionToken = IssueFor(PlayerOwnerKind, playerId, username);

            return sessionToken;
        }

        public string IssueForGuest(int guestId, string guestAlias)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(guestId, 0);
            ArgumentException.ThrowIfNullOrWhiteSpace(guestAlias);

            string sessionToken = IssueFor(GuestOwnerKind, guestId, guestAlias);

            return sessionToken;
        }
        
        internal string IssueForMatch(int matchId, SessionOwner owner, TimeSpan lifetime)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(matchId);
            ArgumentNullException.ThrowIfNull(owner);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lifetime, TimeSpan.Zero);

            string ownerKind = owner.IsGuest ? GuestOwnerKind : PlayerOwnerKind;
            long expiresAtSeconds = _timeProvider.GetUtcNow().Add(lifetime).ToUnixTimeSeconds();
            string payload = string.Format(CultureInfo.InvariantCulture, ReconnectionPayloadFormat, ReconnectionKind, matchId, ownerKind, owner.OwnerId, expiresAtSeconds, owner.DisplayName);
            string reconnectionToken = Sign(payload);

            return reconnectionToken;
        }

        internal SessionTokenReading Read(string sessionToken)
        {
            string[] tokenParts = sessionToken.Split(PartSeparator);
            bool isSigned = (tokenParts.Length == TokenPartCount) && HasValidSignature(tokenParts[0], tokenParts[1]);
            SessionTokenReading tokenReading = isSigned ? ReadPayload(tokenParts[0]) : _invalidReading;

            return tokenReading;
        }

        private static int? ReadOwnerId(string ownerKind, string ownerIdText)
        {
            bool isPlayerOwner = string.Equals(ownerKind, PlayerOwnerKind, StringComparison.Ordinal);
            bool isGuestOwner = string.Equals(ownerKind, GuestOwnerKind, StringComparison.Ordinal);
            NumberStyles ownerIdStyle = isGuestOwner ? NumberStyles.AllowLeadingSign : NumberStyles.None;
            bool isNumericOwnerId = int.TryParse(ownerIdText, ownerIdStyle, CultureInfo.InvariantCulture, out int parsedOwnerId);
            bool isValidPlayerId = isPlayerOwner && (parsedOwnerId > 0);
            bool isValidGuestId = isGuestOwner && (parsedOwnerId < 0);
            int? ownerId = null;
            if (isNumericOwnerId && (isValidPlayerId || isValidGuestId))
            {
                ownerId = parsedOwnerId;
            }

            return ownerId;
        }

        private string IssueFor(string ownerKind, int ownerId, string displayName)
        {
            long expiresAtSeconds = _timeProvider.GetUtcNow().Add(_tokenLifetime).ToUnixTimeSeconds();
            string payload = string.Format(CultureInfo.InvariantCulture, PayloadFormat, ownerKind, ownerId, expiresAtSeconds, displayName);
            string sessionToken = Sign(payload);

            return sessionToken;
        }

        private string Sign(string payload)
        {
            string encodedPayload = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(payload));
            string encodedSignature = Base64Url.EncodeToString(ComputeSignature(encodedPayload));
            string signedToken = encodedPayload + PartSeparator + encodedSignature;

            return signedToken;
        }

        private bool HasValidSignature(string encodedPayload, string encodedSignature)
        {
            byte[] expectedSignature = ComputeSignature(encodedPayload);
            bool isValidSignature = Base64Url.IsValid(encodedSignature)
                && CryptographicOperations.FixedTimeEquals(expectedSignature, Base64Url.DecodeFromChars(encodedSignature));

            return isValidSignature;
        }

        private SessionTokenReading ReadPayload(string encodedPayload)
        {
            string payload = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(encodedPayload));
            bool isReconnection = payload.StartsWith(ReconnectionKind + FieldSeparator, StringComparison.Ordinal);
            SessionTokenReading tokenReading = isReconnection ? ReadReconnectionPayload(payload) : ReadSessionPayload(payload);

            return tokenReading;
        }

        private SessionTokenReading ReadSessionPayload(string payload)
        {
            string[] payloadFields = payload.Split(FieldSeparator, PayloadFieldCount);
            SessionTokenReading tokenReading = _invalidReading;
            if (payloadFields.Length == PayloadFieldCount)
            {
                tokenReading = ReadFields(payloadFields, SessionOwnerFieldsStart);
            }

            return tokenReading;
        }

        private SessionTokenReading ReadReconnectionPayload(string payload)
        {
            string[] payloadFields = payload.Split(FieldSeparator, ReconnectionFieldCount);
            SessionTokenReading tokenReading = _invalidReading;
            if ((payloadFields.Length == ReconnectionFieldCount)
                && int.TryParse(payloadFields[ReconnectionMatchIdField], NumberStyles.None, CultureInfo.InvariantCulture, out int matchId)
                && (matchId > 0))
            {
                SessionTokenReading ownerReading = ReadFields(payloadFields, ReconnectionOwnerFieldsStart);
                bool isOwnerRead = ownerReading.Status != SessionTokenStatus.Invalid;
                tokenReading = isOwnerRead ? ownerReading with { MatchId = matchId } : _invalidReading;
            }

            return tokenReading;
        }

        private SessionTokenReading ReadFields(string[] payloadFields, int ownerFieldsStart)
        {
            string ownerKind = payloadFields[ownerFieldsStart + OwnerKindField];
            string displayName = payloadFields[ownerFieldsStart + DisplayNameField];
            int? ownerId = ReadOwnerId(ownerKind, payloadFields[ownerFieldsStart + OwnerIdField]);
            bool hasValidExpiry = long.TryParse(payloadFields[ownerFieldsStart + ExpiryField], NumberStyles.None, CultureInfo.InvariantCulture, out long expiresAtSeconds);
            bool hasValidFields = (ownerId is not null) && hasValidExpiry && !string.IsNullOrWhiteSpace(displayName);
            SessionTokenReading tokenReading = _invalidReading;
            if (hasValidFields)
            {
                bool isGuestOwner = string.Equals(ownerKind, GuestOwnerKind, StringComparison.Ordinal);
                bool hasExpired = _timeProvider.GetUtcNow().ToUnixTimeSeconds() > expiresAtSeconds;
                SessionTokenStatus tokenStatus = hasExpired ? SessionTokenStatus.Expired : SessionTokenStatus.Valid;
                tokenReading = new SessionTokenReading(tokenStatus, ownerId.GetValueOrDefault(), displayName, isGuestOwner);
            }

            return tokenReading;
        }

        private byte[] ComputeSignature(string encodedPayload)
        {
            byte[] signature = HMACSHA256.HashData(_signingKey, Encoding.UTF8.GetBytes(encodedPayload));

            return signature;
        }
    }
}
