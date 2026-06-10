using PropertyViewerAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PropertyViewerAccounting.Infrastructure.Data.Configurations;

public class BillProviderConfiguration : IEntityTypeConfiguration<BillProvider>
{
    public void Configure(EntityTypeBuilder<BillProvider> builder)
    {
        builder.ToTable("bill_providers");

        builder.HasKey(bp => bp.BillProviderId);
        builder.Property(bp => bp.BillProviderId).HasColumnName("bill_provider_id");

        builder.Property(bp => bp.BillId)
            .HasColumnName("bill_id")
            .IsRequired();

        builder.Property(bp => bp.ProviderId)
            .HasColumnName("provider_id")
            .IsRequired();

        builder.HasIndex(bp => new { bp.BillId, bp.ProviderId }).IsUnique();

        builder.Property(bp => bp.BillProviderAccountNumber)
            .HasColumnName("bill_provider_account_number")
            .HasMaxLength(100);

        builder.Property(bp => bp.BillProviderInvoiceNumber)
            .HasColumnName("bill_provider_invoice_number")
            .HasMaxLength(100);

        builder.Property(bp => bp.BillProviderNotes)
            .HasColumnName("bill_provider_notes")
            .HasMaxLength(2000);

        builder.Property(bp => bp.BillProviderIsPrimary)
            .HasColumnName("bill_provider_is_primary")
            .HasDefaultValue(false);

        builder.Property(bp => bp.BillProviderIsActive)
            .HasColumnName("bill_provider_is_active")
            .HasDefaultValue(true);

        builder.Property(bp => bp.BillProviderCreatedAt)
            .HasColumnName("bill_provider_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(bp => bp.BillProviderUpdatedAt)
            .HasColumnName("bill_provider_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(bp => bp.Bill)
            .WithMany(b => b.BillProviders)
            .HasForeignKey(bp => bp.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bp => bp.Provider)
            .WithMany(p => p.BillProviders)
            .HasForeignKey(bp => bp.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
