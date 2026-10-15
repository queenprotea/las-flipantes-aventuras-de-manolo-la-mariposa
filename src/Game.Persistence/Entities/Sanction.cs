using System;

namespace Game.Persistence
{
    public sealed class Sanction
    {
        public int SanctionId { get; set; }

        public int PlayerId { get; set; }

        public short? Level { get; set; }

        public bool IsPermanent { get; set; }

        public DateTime ThresholdAt { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveUntil { get; set; }
    }
}
