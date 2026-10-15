using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class FriendshipMapping : IEntityTypeConfiguration<Friendship>
    {
        private const string FriendshipTable = "friendship";
        private const string RequesterIdColumn = "requester_id";
        private const string AddresseeIdColumn = "addressee_id";
        private const string StatusColumn = "status";
        private const string RequestedAtColumn = "requested_at";
        private const string RespondedAtColumn = "responded_at";

        public void Configure(EntityTypeBuilder<Friendship> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable(FriendshipTable);
            builder.HasKey(mappedFriendship => new { mappedFriendship.RequesterId, mappedFriendship.AddresseeId });
            builder.Property(mappedFriendship => mappedFriendship.RequesterId).HasColumnName(RequesterIdColumn);
            builder.Property(mappedFriendship => mappedFriendship.AddresseeId).HasColumnName(AddresseeIdColumn);
            builder.Property(mappedFriendship => mappedFriendship.Status).HasColumnName(StatusColumn);
            builder.Property(mappedFriendship => mappedFriendship.RequestedAt)
                .HasColumnName(RequestedAtColumn)
                .ValueGeneratedOnAdd();
            builder.Property(mappedFriendship => mappedFriendship.RespondedAt).HasColumnName(RespondedAtColumn);
        }
    }
}
