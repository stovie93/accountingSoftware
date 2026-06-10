using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class PropertyUtilityBillConfiguration : IEntityTypeConfiguration<PropertyUtilityBill>
{
    public void Configure(EntityTypeBuilder<PropertyUtilityBill> builder)
    {
        builder.ToTable("property_utility_bills");

        builder.HasKey(pub => pub.PropertyUtilityBillId);
        builder.Property(pub => pub.PropertyUtilityBillId).HasColumnName("property_utility_bill_id");

        builder.Property(pub => pub.PropertyUtilityId)
            .HasColumnName("property_utility_id")
            .IsRequired();

        builder.Property(pub => pub.BillId)
            .HasColumnName("bill_id")
            .IsRequired();

        builder.HasIndex(pub => new { pub.PropertyUtilityId, pub.BillId }).IsUnique();

        builder.Property(pub => pub.PropertyUtilityBillAmount)
            .HasColumnName("property_utility_bill_amount")
            .HasPrecision(18, 2);

        builder.Property(pub => pub.PropertyUtilityBillQuantity)
            .HasColumnName("property_utility_bill_quantity")
            .HasPrecision(18, 4);

        builder.Property(pub => pub.PropertyUtilityBillPeriod)
            .HasColumnName("property_utility_bill_period")
            .HasMaxLength(50);

        builder.Property(pub => pub.PropertyUtilityBillNotes)
            .HasColumnName("property_utility_bill_notes")
            .HasMaxLength(2000);

        builder.Property(pub => pub.PropertyUtilityBillIsActive)
            .HasColumnName("property_utility_bill_is_active")
            .HasDefaultValue(true);

        builder.Property(pub => pub.PropertyUtilityBillCreatedAt)
            .HasColumnName("property_utility_bill_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(pub => pub.PropertyUtilityBillUpdatedAt)
            .HasColumnName("property_utility_bill_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(pub => pub.PropertyUtility)
            .WithMany(pu => pu.PropertyUtilityBills)
            .HasForeignKey(pub => pub.PropertyUtilityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pub => pub.Bill)
            .WithMany()
            .HasForeignKey(pub => pub.BillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
