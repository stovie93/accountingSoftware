using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class ProviderUtilityConfiguration : IEntityTypeConfiguration<ProviderUtility>
{
    public void Configure(EntityTypeBuilder<ProviderUtility> builder)
    {
        builder.ToTable("provider_utilities");

        builder.HasKey(pu => pu.ProviderUtilityId);
        builder.Property(pu => pu.ProviderUtilityId).HasColumnName("provider_utility_id");

        builder.Property(pu => pu.ProviderId)
            .HasColumnName("provider_id")
            .IsRequired();

        builder.Property(pu => pu.UtilityId)
            .HasColumnName("utility_id")
            .IsRequired();

        builder.HasIndex(pu => new { pu.ProviderId, pu.UtilityId }).IsUnique();

        builder.Property(pu => pu.ProviderUtilityServiceArea)
            .HasColumnName("provider_utility_service_area")
            .HasMaxLength(256);

        builder.Property(pu => pu.ProviderUtilityDefaultRate)
            .HasColumnName("provider_utility_default_rate")
            .HasPrecision(18, 6);

        builder.Property(pu => pu.ProviderUtilityRateUnit)
            .HasColumnName("provider_utility_rate_unit")
            .HasMaxLength(50);

        builder.Property(pu => pu.ProviderUtilityNotes)
            .HasColumnName("provider_utility_notes")
            .HasMaxLength(2000);

        builder.Property(pu => pu.ProviderUtilityIsPrimary)
            .HasColumnName("provider_utility_is_primary")
            .HasDefaultValue(false);

        builder.Property(pu => pu.ProviderUtilityIsActive)
            .HasColumnName("provider_utility_is_active")
            .HasDefaultValue(true);

        builder.Property(pu => pu.ProviderUtilityCreatedAt)
            .HasColumnName("provider_utility_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(pu => pu.ProviderUtilityUpdatedAt)
            .HasColumnName("provider_utility_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(pu => pu.Provider)
            .WithMany(p => p.ProviderUtilities)
            .HasForeignKey(pu => pu.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pu => pu.Utility)
            .WithMany(u => u.ProviderUtilities)
            .HasForeignKey(pu => pu.UtilityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
