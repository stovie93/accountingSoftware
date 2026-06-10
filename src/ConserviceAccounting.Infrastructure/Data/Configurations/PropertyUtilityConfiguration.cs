using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class PropertyUtilityConfiguration : IEntityTypeConfiguration<PropertyUtility>
{
    public void Configure(EntityTypeBuilder<PropertyUtility> builder)
    {
        builder.ToTable("property_utilities");

        builder.HasKey(pu => pu.PropertyUtilityId);
        builder.Property(pu => pu.PropertyUtilityId).HasColumnName("property_utility_id");

        builder.Property(pu => pu.PropertyId)
            .HasColumnName("property_id")
            .IsRequired();

        builder.Property(pu => pu.UtilityId)
            .HasColumnName("utility_id")
            .IsRequired();

        builder.HasIndex(pu => new { pu.PropertyId, pu.UtilityId }).IsUnique();

        builder.Property(pu => pu.PropertyUtilityAccountNumber)
            .HasColumnName("property_utility_account_number")
            .HasMaxLength(100);

        builder.Property(pu => pu.PropertyUtilityMeterNumber)
            .HasColumnName("property_utility_meter_number")
            .HasMaxLength(100);

        builder.Property(pu => pu.PropertyUtilityServiceAddress)
            .HasColumnName("property_utility_service_address")
            .HasMaxLength(500);

        builder.Property(pu => pu.PropertyUtilityStartDate)
            .HasColumnName("property_utility_start_date");

        builder.Property(pu => pu.PropertyUtilityEndDate)
            .HasColumnName("property_utility_end_date");

        builder.Property(pu => pu.PropertyUtilityBudget)
            .HasColumnName("property_utility_budget")
            .HasPrecision(18, 2);

        builder.Property(pu => pu.PropertyUtilityNotes)
            .HasColumnName("property_utility_notes")
            .HasMaxLength(2000);

        builder.Property(pu => pu.PropertyUtilityIsActive)
            .HasColumnName("property_utility_is_active")
            .HasDefaultValue(true);

        builder.Property(pu => pu.PropertyUtilityCreatedAt)
            .HasColumnName("property_utility_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(pu => pu.PropertyUtilityUpdatedAt)
            .HasColumnName("property_utility_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(pu => pu.Property)
            .WithMany(p => p.PropertyUtilities)
            .HasForeignKey(pu => pu.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pu => pu.Utility)
            .WithMany(u => u.PropertyUtilities)
            .HasForeignKey(pu => pu.UtilityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
