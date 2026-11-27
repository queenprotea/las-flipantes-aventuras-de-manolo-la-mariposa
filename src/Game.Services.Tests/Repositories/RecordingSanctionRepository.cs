using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal sealed class RecordingSanctionRepository : ISanctionRepository
    {
        internal List<Sanction> Sanctions { get; } = new List<Sanction>();

        public Task<Sanction?> FindInForceAsync(int playerId, DateTime nowUtc)
        {
            Sanction? sanctionInForce = Sanctions.Find(sanction => (sanction.PlayerId == playerId) && (sanction.EffectiveFrom <= nowUtc) && ((sanction.EffectiveUntil is null) || (sanction.EffectiveUntil > nowUtc)));
            Task<Sanction?> finding = Task.FromResult(sanctionInForce);

            return finding;
        }

        public Task<bool> HasPermanentAsync(int playerId)
        {
            Task<bool> checking = Task.FromResult(Sanctions.Exists(sanction => (sanction.PlayerId == playerId) && sanction.IsPermanent));

            return checking;
        }

        public Task<DateTime?> FindLatestThresholdAsync(int playerId)
        {
            DateTime? latestThreshold = Sanctions.Where(sanction => sanction.PlayerId == playerId).Max(sanction => (DateTime?)sanction.ThresholdAt);
            Task<DateTime?> finding = Task.FromResult(latestThreshold);

            return finding;
        }

        public Task<int> CountAsync(int playerId)
        {
            Task<int> counting = Task.FromResult(Sanctions.Count(sanction => sanction.PlayerId == playerId));

            return counting;
        }

        public Task CreateAsync(Sanction newSanction)
        {
            Sanctions.Add(newSanction);
            Task creation = Task.CompletedTask;

            return creation;
        }
    }
}
