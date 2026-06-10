using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class PropertyProviderConfiguration : IEntityTypeConfiguration<PropertyProvider>
{
    public void Configure(EntityTypeBuilder<PropertyProvider> builder)
    {
        builder.ToTable("property_providers");

        builder.HasKey(pp => pp.PropertyProviderId);
        builder.Property(pp => pp.PropertyProviderId).HasColumnName("property_provider_id");

        builder.Property(pp => pp.PropertyId)
            .HasColumnName("property_id")
            .IsRequired();

        builder.Property(pp => pp.ProviderId)
            .HasColumnName("provider_id")
            .IsRequired();

        builder.HasIndex(pp => new { pp.PropertyId, pp.ProviderId }).IsUnique();

        builder.Property(pp => pp.PropertyProviderAccountNumber)
            .HasColumnName("property_provider_account_number")
            .HasMaxLength(100);

        builder.Property(pp => pp.PropertyProviderServiceType)
            .HasColumnName("property_provider_service_type")
            .HasMaxLength(100);

        builder.Property(pp => pp.PropertyProviderStartDate)
            .HasColumnName("property_provider_start_date");

        builder.Property(pp => pp.PropertyProviderEndDate)
            .HasColumnName("property_provider_end_date");

        builder.Property(pp => pp.PropertyProviderContractNumber)
            .HasColumnName("property_provider_contract_number")
            .HasMaxLength(100);

        builder.Property(pp => pp.PropertyProviderNotes)
            .HasColumnName("property_provider_notes")
            .HasMaxLength(2000);

        builder.Property(pp => pp.PropertyProviderIsPrimary)
            .HasColumnName("property_provider_is_primary")
            .HasDefaultValue(false);

        builder.Property(pp => pp.PropertyProviderIsActive)
            .HasColumnName("property_provider_is_active")
            .HasDefaultValue(true);

        builder.Property(pp => pp.PropertyProviderCreatedAt)
            .HasColumnName("property_provider_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(pp => pp.PropertyProviderUpdatedAt)
            .HasColumnName("property_provider_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(pp => pp.Property)
            .WithMany(p => p.PropertyProviders)
            .HasForeignKey(pp => pp.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pp => pp.Provider)
            .WithMany(p => p.PropertyProviders)
            .HasForeignKey(pp => pp.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
