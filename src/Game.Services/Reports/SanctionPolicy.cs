using System;
using System.Collections.Generic;
using System.Linq;

using Game.Persistence;

namespace Game.Services
{
    public sealed class SanctionPolicy
    {
        private readonly IReadOnlyList<TimeSpan> _temporaryBanDurations;

        public SanctionPolicy(int thresholdOccasions, IReadOnlyList<TimeSpan> temporaryBanDurations)
        {
            ThresholdOccasions = thresholdOccasions;
            _temporaryBanDurations = temporaryBanDurations.ToList();
        }

        public int ThresholdOccasions { get; }

        public Sanction BuildSanction(int playerId, int previousSanctionCount, DateTime thresholdAt)
        {
            bool isPermanent = previousSanctionCount >= _temporaryBanDurations.Count;
            var newSanction = new Sanction
            {
                PlayerId = playerId,
                IsPermanent = isPermanent,
                ThresholdAt = thresholdAt,
                EffectiveFrom = thresholdAt,
            };
            if (!isPermanent)
            {
                newSanction.Level = (short)(previousSanctionCount + 1);
                newSanction.EffectiveUntil = thresholdAt + _temporaryBanDurations[previousSanctionCount];
            }

            return newSanction;
        }
    }
}
