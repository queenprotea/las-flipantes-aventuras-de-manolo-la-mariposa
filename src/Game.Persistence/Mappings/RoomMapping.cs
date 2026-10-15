using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class RoomMapping : IEntityTypeConfiguration<Room>
    {
        private const string RoomTable = "room";
        private const string RoomIdColumn = "room_id";
        private const string CodeColumn = "code";
        private const string VisibilityColumn = "visibility";
        private const string CreatedAtColumn = "created_at";
        private const string ClosedAtColumn = "closed_at";

        public void Configure(EntityTypeBuilder<Room> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable(RoomTable);
            builder.HasKey(mappedRoom => mappedRoom.RoomId);
            builder.Property(mappedRoom => mappedRoom.RoomId)
                .HasColumnName(RoomIdColumn)
                .ValueGeneratedOnAdd();
            builder.Property(mappedRoom => mappedRoom.Code).HasColumnName(CodeColumn);
            builder.Property(mappedRoom => mappedRoom.Visibility).HasColumnName(VisibilityColumn);
            builder.Property(mappedRoom => mappedRoom.CreatedAt)
                .HasColumnName(CreatedAtColumn)
                .ValueGeneratedOnAdd();
            builder.Property(mappedRoom => mappedRoom.ClosedAt).HasColumnName(ClosedAtColumn);
        }
    }
}
