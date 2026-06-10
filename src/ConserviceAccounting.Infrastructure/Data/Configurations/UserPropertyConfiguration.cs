using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class UserPropertyConfiguration : IEntityTypeConfiguration<UserProperty>
{
    public void Configure(EntityTypeBuilder<UserProperty> builder)
    {
        builder.ToTable("user_properties");

        builder.HasKey(up => up.UserPropertyId);
        builder.Property(up => up.UserPropertyId).HasColumnName("user_property_id");

        builder.Property(up => up.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(up => up.PropertyId)
            .HasColumnName("property_id")
            .IsRequired();

        builder.HasIndex(up => new { up.UserId, up.PropertyId }).IsUnique();

        builder.Property(up => up.UserPropertyRole)
            .HasColumnName("user_property_role")
            .HasMaxLength(100);

        builder.Property(up => up.UserPropertyAccessLevel)
            .HasColumnName("user_property_access_level")
            .HasMaxLength(50);

        builder.Property(up => up.UserPropertyIsPrimary)
            .HasColumnName("user_property_is_primary")
            .HasDefaultValue(false);

        builder.Property(up => up.UserPropertyAssignedAt)
            .HasColumnName("user_property_assigned_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(up => up.UserPropertyExpiresAt)
            .HasColumnName("user_property_expires_at");

        builder.Property(up => up.UserPropertyNotes)
            .HasColumnName("user_property_notes")
            .HasMaxLength(2000);

        builder.Property(up => up.UserPropertyIsActive)
            .HasColumnName("user_property_is_active")
            .HasDefaultValue(true);

        builder.Property(up => up.UserPropertyCreatedAt)
            .HasColumnName("user_property_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(up => up.UserPropertyUpdatedAt)
            .HasColumnName("user_property_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(up => up.User)
            .WithMany(u => u.UserProperties)
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(up => up.Property)
            .WithMany(p => p.UserProperties)
            .HasForeignKey(up => up.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
