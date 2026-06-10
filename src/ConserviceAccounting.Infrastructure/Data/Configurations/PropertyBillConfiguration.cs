using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class PropertyBillConfiguration : IEntityTypeConfiguration<PropertyBill>
{
    public void Configure(EntityTypeBuilder<PropertyBill> builder)
    {
        builder.ToTable("property_bills");

        builder.HasKey(pb => pb.PropertyBillId);
        builder.Property(pb => pb.PropertyBillId).HasColumnName("property_bill_id");

        builder.Property(pb => pb.PropertyId)
            .HasColumnName("property_id")
            .IsRequired();

        builder.Property(pb => pb.BillId)
            .HasColumnName("bill_id")
            .IsRequired();

        builder.HasIndex(pb => new { pb.PropertyId, pb.BillId }).IsUnique();

        builder.Property(pb => pb.PropertyBillAllocationMethod)
            .HasColumnName("property_bill_allocation_method")
            .HasMaxLength(50);

        builder.Property(pb => pb.PropertyBillAllocationPercent)
            .HasColumnName("property_bill_allocation_percent")
            .HasPrecision(5, 2);

        builder.Property(pb => pb.PropertyBillAllocationAmount)
            .HasColumnName("property_bill_allocation_amount")
            .HasPrecision(18, 2);

        builder.Property(pb => pb.PropertyBillCostCenter)
            .HasColumnName("property_bill_cost_center")
            .HasMaxLength(100);

        builder.Property(pb => pb.PropertyBillGLCode)
            .HasColumnName("property_bill_gl_code")
            .HasMaxLength(50);

        builder.Property(pb => pb.PropertyBillNotes)
            .HasColumnName("property_bill_notes")
            .HasMaxLength(2000);

        builder.Property(pb => pb.PropertyBillIsPrimary)
            .HasColumnName("property_bill_is_primary")
            .HasDefaultValue(false);

        builder.Property(pb => pb.PropertyBillIsActive)
            .HasColumnName("property_bill_is_active")
            .HasDefaultValue(true);

        builder.Property(pb => pb.PropertyBillCreatedAt)
            .HasColumnName("property_bill_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(pb => pb.PropertyBillUpdatedAt)
            .HasColumnName("property_bill_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(pb => pb.Property)
            .WithMany(p => p.PropertyBills)
            .HasForeignKey(pb => pb.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pb => pb.Bill)
            .WithMany(b => b.PropertyBills)
            .HasForeignKey(pb => pb.BillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
