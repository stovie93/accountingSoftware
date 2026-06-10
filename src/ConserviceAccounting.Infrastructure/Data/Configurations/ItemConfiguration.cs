using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");

        builder.Property(i => i.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(i => i.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(256);
        builder.HasIndex(i => new { i.UserId, i.ExternalId });

        builder.Property(i => i.Date)
            .HasColumnName("date")
            .IsRequired();
        builder.HasIndex(i => new { i.UserId, i.Date });

        builder.Property(i => i.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .HasColumnName("quantity")
            .HasPrecision(18, 4);

        builder.Property(i => i.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(i => i.Attributes)
            .HasColumnName("attributes")
            .HasColumnType("jsonb");

        builder.Property(i => i.Source)
            .HasColumnName("source")
            .HasConversion<int>();

        builder.Property(i => i.BatchId)
            .HasColumnName("batch_id");
        builder.HasIndex(i => i.BatchId);

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite index for filtering by source
        builder.HasIndex(i => new { i.UserId, i.Source })
            .HasDatabaseName("ix_items_user_source");

        // Composite index for date range + source queries
        builder.HasIndex(i => new { i.UserId, i.Source, i.Date })
            .HasDatabaseName("ix_items_user_source_date");

        // Items are linked to entities via ItemEntity bridge table
        // No direct navigation to entities here - use ItemEntities collection
    }
}
