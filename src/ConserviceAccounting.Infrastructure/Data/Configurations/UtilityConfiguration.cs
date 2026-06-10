using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class UtilityConfiguration : IEntityTypeConfiguration<Utility>
{
    public void Configure(EntityTypeBuilder<Utility> builder)
    {
        builder.ToTable("utilities");

        builder.HasKey(u => u.UtilityId);
        builder.Property(u => u.UtilityId).HasColumnName("utility_id");

        builder.Property(u => u.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(u => u.UtilityName)
            .HasColumnName("utility_name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.UtilityType)
            .HasColumnName("utility_type")
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(u => new { u.UserId, u.UtilityType });

        builder.Property(u => u.UtilityCode)
            .HasColumnName("utility_code")
            .HasMaxLength(50);
        builder.HasIndex(u => new { u.UserId, u.UtilityCode }).IsUnique();

        builder.Property(u => u.UtilityDescription)
            .HasColumnName("utility_description")
            .HasMaxLength(2000);

        builder.Property(u => u.UtilityAttributes)
            .HasColumnName("utility_attributes")
            .HasColumnType("jsonb");

        builder.Property(u => u.UtilityIsActive)
            .HasColumnName("utility_is_active")
            .HasDefaultValue(true);

        builder.Property(u => u.UtilityCreatedAt)
            .HasColumnName("utility_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UtilityUpdatedAt)
            .HasColumnName("utility_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for common queries
        builder.HasIndex(u => new { u.UserId, u.UtilityIsActive });
    }
}
