using PropertyViewerAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PropertyViewerAccounting.Infrastructure.Data.Configurations;

public class EntityTypeConfiguration : IEntityTypeConfiguration<EntityType>
{
    public void Configure(EntityTypeBuilder<EntityType> builder)
    {
        builder.ToTable("entity_types");

        builder.HasKey(et => et.Id);
        builder.Property(et => et.Id).HasColumnName("id");

        builder.Property(et => et.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(et => et.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(et => et.Code)
            .HasColumnName("code")
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(et => new { et.UserId, et.Code }).IsUnique();

        builder.Property(et => et.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(et => et.Color)
            .HasColumnName("color")
            .HasMaxLength(50);

        builder.Property(et => et.Icon)
            .HasColumnName("icon")
            .HasMaxLength(100);

        builder.Property(et => et.SortOrder)
            .HasColumnName("sort_order")
            .HasDefaultValue(0);

        builder.Property(et => et.IsSystem)
            .HasColumnName("is_system")
            .HasDefaultValue(false);

        builder.Property(et => et.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(et => et.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(et => et.User)
            .WithMany()
            .HasForeignKey(et => et.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
