using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class RoomInvitationRecordMapping : IEntityTypeConfiguration<RoomInvitationRecord>
    {
        private const string RoomInvitationTable = "room_invitation";
        private const string RoomIdColumn = "room_id";
        private const string InviterPlayerIdColumn = "inviter_player_id";
        private const string InvitedPlayerIdColumn = "invited_player_id";
        private const string ChannelColumn = "channel";
        private const string CreatedAtColumn = "created_at";

        public void Configure(EntityTypeBuilder<RoomInvitationRecord> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable(RoomInvitationTable);
            builder.HasKey(mappedInvitation => new { mappedInvitation.RoomId, mappedInvitation.InvitedPlayerId });
            builder.Property(mappedInvitation => mappedInvitation.RoomId).HasColumnName(RoomIdColumn);
            builder.Property(mappedInvitation => mappedInvitation.InviterPlayerId).HasColumnName(InviterPlayerIdColumn);
            builder.Property(mappedInvitation => mappedInvitation.InvitedPlayerId).HasColumnName(InvitedPlayerIdColumn);
            builder.Property(mappedInvitation => mappedInvitation.Channel).HasColumnName(ChannelColumn);
            builder.Property(mappedInvitation => mappedInvitation.CreatedAt)
                .HasColumnName(CreatedAtColumn)
                .ValueGeneratedOnAdd();
        }
    }
}
