using PropertyViewerAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PropertyViewerAccounting.Infrastructure.Data.Configurations;

public class ItemEntityConfiguration : IEntityTypeConfiguration<ItemEntity>
{
    public void Configure(EntityTypeBuilder<ItemEntity> builder)
    {
        builder.ToTable("item_entities");

        // Composite primary key
        builder.HasKey(ie => new { ie.ItemId, ie.EntityId });

        builder.Property(ie => ie.ItemId)
            .HasColumnName("item_id");

        builder.Property(ie => ie.EntityId)
            .HasColumnName("entity_id");

        builder.Property(ie => ie.AssignedAt)
            .HasColumnName("assigned_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes for efficient querying
        builder.HasIndex(ie => ie.ItemId);
        builder.HasIndex(ie => ie.EntityId);

        builder.HasOne(ie => ie.Item)
            .WithMany(i => i.ItemEntities)
            .HasForeignKey(ie => ie.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ie => ie.Entity)
            .WithMany(e => e.ItemEntities)
            .HasForeignKey(ie => ie.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
