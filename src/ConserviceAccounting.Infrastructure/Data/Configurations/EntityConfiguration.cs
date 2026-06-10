using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class EntityConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("entities");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.EntityTypeId)
            .HasColumnName("entity_type_id")
            .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Code)
            .HasColumnName("code")
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(e => new { e.UserId, e.Code }).IsUnique();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(e => e.Attributes)
            .HasColumnName("attributes")
            .HasColumnType("jsonb");

        builder.Property(e => e.SortOrder)
            .HasColumnName("sort_order")
            .HasDefaultValue(0);

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(e => e.IsSystem)
            .HasColumnName("is_system")
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes for efficient querying
        builder.HasIndex(e => new { e.UserId, e.EntityTypeId });
        builder.HasIndex(e => new { e.UserId, e.IsActive });

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.EntityType)
            .WithMany(et => et.Entities)
            .HasForeignKey(e => e.EntityTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
