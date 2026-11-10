using System;
using System.Collections.Generic;
using System.Linq;

using Game.Persistence;

namespace Game.Services
{
    public sealed class SanctionPolicy
    {
        private const string MissingTemporaryBanDurationsMessage = "At least one temporary ban duration is required.";
        private const string NonPositiveTemporaryBanDurationMessage = "Every temporary ban duration must be longer than zero.";

        private readonly IReadOnlyList<TimeSpan> _temporaryBanDurations;

        public SanctionPolicy(int thresholdOccasions, IReadOnlyList<TimeSpan> temporaryBanDurations)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(thresholdOccasions);
            ArgumentNullException.ThrowIfNull(temporaryBanDurations);
            if (temporaryBanDurations.Count == 0)
            {
                throw new ArgumentException(MissingTemporaryBanDurationsMessage, nameof(temporaryBanDurations));
            }

            if (temporaryBanDurations.Any(banDuration => banDuration <= TimeSpan.Zero))
            {
                throw new ArgumentException(NonPositiveTemporaryBanDurationMessage, nameof(temporaryBanDurations));
            }

            ThresholdOccasions = thresholdOccasions;
            _temporaryBanDurations = temporaryBanDurations.ToList();
        }

        public int ThresholdOccasions { get; }

        public Sanction BuildSanction(int playerId, int previousSanctionCount, DateTime thresholdAtUtc)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(playerId);
            ArgumentOutOfRangeException.ThrowIfNegative(previousSanctionCount);

            bool isPermanent = previousSanctionCount >= _temporaryBanDurations.Count;
            var newSanction = new Sanction
            {
                PlayerId = playerId,
                IsPermanent = isPermanent,
                ThresholdAt = thresholdAtUtc,
                EffectiveFrom = thresholdAtUtc,
            };
            if (!isPermanent)
            {
                newSanction.Level = (short)(previousSanctionCount + 1);
                newSanction.EffectiveUntil = thresholdAtUtc + _temporaryBanDurations[previousSanctionCount];
            }

            return newSanction;
        }
    }
}
