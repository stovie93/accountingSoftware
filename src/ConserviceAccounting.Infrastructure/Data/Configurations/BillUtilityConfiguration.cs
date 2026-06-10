using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class BillUtilityConfiguration : IEntityTypeConfiguration<BillUtility>
{
    public void Configure(EntityTypeBuilder<BillUtility> builder)
    {
        builder.ToTable("bill_utilities");

        builder.HasKey(bu => bu.BillUtilityId);
        builder.Property(bu => bu.BillUtilityId).HasColumnName("bill_utility_id");

        builder.Property(bu => bu.BillId)
            .HasColumnName("bill_id")
            .IsRequired();

        builder.Property(bu => bu.UtilityId)
            .HasColumnName("utility_id")
            .IsRequired();

        builder.HasIndex(bu => new { bu.BillId, bu.UtilityId }).IsUnique();

        builder.Property(bu => bu.BillUtilityAmount)
            .HasColumnName("bill_utility_amount")
            .HasPrecision(18, 2);

        builder.Property(bu => bu.BillUtilityQuantity)
            .HasColumnName("bill_utility_quantity")
            .HasPrecision(18, 4);

        builder.Property(bu => bu.BillUtilityUnit)
            .HasColumnName("bill_utility_unit")
            .HasMaxLength(50);

        builder.Property(bu => bu.BillUtilityRate)
            .HasColumnName("bill_utility_rate")
            .HasPrecision(18, 6);

        builder.Property(bu => bu.BillUtilityNotes)
            .HasColumnName("bill_utility_notes")
            .HasMaxLength(2000);

        builder.Property(bu => bu.BillUtilityIsPrimary)
            .HasColumnName("bill_utility_is_primary")
            .HasDefaultValue(false);

        builder.Property(bu => bu.BillUtilityIsActive)
            .HasColumnName("bill_utility_is_active")
            .HasDefaultValue(true);

        builder.Property(bu => bu.BillUtilityCreatedAt)
            .HasColumnName("bill_utility_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(bu => bu.BillUtilityUpdatedAt)
            .HasColumnName("bill_utility_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(bu => bu.Bill)
            .WithMany(b => b.BillUtilities)
            .HasForeignKey(bu => bu.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bu => bu.Utility)
            .WithMany(u => u.BillUtilities)
            .HasForeignKey(bu => bu.UtilityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
