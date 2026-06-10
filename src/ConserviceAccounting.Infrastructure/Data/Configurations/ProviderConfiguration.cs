using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("providers");

        builder.HasKey(p => p.ProviderId);
        builder.Property(p => p.ProviderId).HasColumnName("provider_id");

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.ProviderAccountId)
            .HasColumnName("provider_account_id")
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(p => new { p.UserId, p.ProviderAccountId }).IsUnique();

        builder.Property(p => p.ProviderName)
            .HasColumnName("provider_name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(p => p.ProviderCode)
            .HasColumnName("provider_code")
            .HasMaxLength(50);

        builder.Property(p => p.ProviderDescription)
            .HasColumnName("provider_description")
            .HasMaxLength(2000);

        builder.Property(p => p.ProviderType)
            .HasColumnName("provider_type")
            .HasMaxLength(100);

        builder.Property(p => p.ProviderAddress)
            .HasColumnName("provider_address")
            .HasMaxLength(500);

        builder.Property(p => p.ProviderCity)
            .HasColumnName("provider_city")
            .HasMaxLength(100);

        builder.Property(p => p.ProviderState)
            .HasColumnName("provider_state")
            .HasMaxLength(100);

        builder.Property(p => p.ProviderZipCode)
            .HasColumnName("provider_zip_code")
            .HasMaxLength(20);

        builder.Property(p => p.ProviderCountry)
            .HasColumnName("provider_country")
            .HasMaxLength(100);

        builder.Property(p => p.ProviderPhone)
            .HasColumnName("provider_phone")
            .HasMaxLength(50);

        builder.Property(p => p.ProviderEmail)
            .HasColumnName("provider_email")
            .HasMaxLength(256);

        builder.Property(p => p.ProviderWebsite)
            .HasColumnName("provider_website")
            .HasMaxLength(500);

        builder.Property(p => p.ProviderContactName)
            .HasColumnName("provider_contact_name")
            .HasMaxLength(200);

        builder.Property(p => p.ProviderAccountNumber)
            .HasColumnName("provider_account_number")
            .HasMaxLength(100);

        builder.Property(p => p.ProviderAttributes)
            .HasColumnName("provider_attributes")
            .HasColumnType("jsonb");

        builder.Property(p => p.ProviderIsActive)
            .HasColumnName("provider_is_active")
            .HasDefaultValue(true);

        builder.Property(p => p.ProviderCreatedAt)
            .HasColumnName("provider_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.ProviderUpdatedAt)
            .HasColumnName("provider_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for common queries
        builder.HasIndex(p => new { p.UserId, p.ProviderType });
        builder.HasIndex(p => new { p.UserId, p.ProviderIsActive });
    }
}
