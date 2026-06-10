using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.ToTable("bills");

        builder.HasKey(b => b.BillId);
        builder.Property(b => b.BillId).HasColumnName("bill_id");

        builder.Property(b => b.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(b => b.BillReferenceId)
            .HasColumnName("bill_reference_id")
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(b => new { b.UserId, b.BillReferenceId }).IsUnique();

        builder.Property(b => b.BillAmount)
            .HasColumnName("bill_amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.BillStatus)
            .HasColumnName("bill_status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(b => b.BillDescription)
            .HasColumnName("bill_description")
            .HasMaxLength(2000);

        builder.Property(b => b.BillDate)
            .HasColumnName("bill_date");

        builder.Property(b => b.BillDueDate)
            .HasColumnName("bill_due_date");

        builder.Property(b => b.BillPaidDate)
            .HasColumnName("bill_paid_date");

        builder.Property(b => b.BillPeriodStart)
            .HasColumnName("bill_period_start")
            .HasMaxLength(50);

        builder.Property(b => b.BillPeriodEnd)
            .HasColumnName("bill_period_end")
            .HasMaxLength(50);

        builder.Property(b => b.BillQuantity)
            .HasColumnName("bill_quantity")
            .HasPrecision(18, 4);

        builder.Property(b => b.BillUnit)
            .HasColumnName("bill_unit")
            .HasMaxLength(50);

        builder.Property(b => b.BillRate)
            .HasColumnName("bill_rate")
            .HasPrecision(18, 6);

        builder.Property(b => b.BillTax)
            .HasColumnName("bill_tax")
            .HasPrecision(18, 2);

        builder.Property(b => b.BillTotalAmount)
            .HasColumnName("bill_total_amount")
            .HasPrecision(18, 2);

        builder.Property(b => b.BillCurrency)
            .HasColumnName("bill_currency")
            .HasMaxLength(10);

        builder.Property(b => b.BillNotes)
            .HasColumnName("bill_notes")
            .HasMaxLength(4000);

        builder.Property(b => b.BillCategory)
            .HasColumnName("bill_category")
            .HasMaxLength(100);

        builder.Property(b => b.BillExternalId)
            .HasColumnName("bill_external_id")
            .HasMaxLength(256);
        builder.HasIndex(b => new { b.UserId, b.BillExternalId });

        builder.Property(b => b.BillAttributes)
            .HasColumnName("bill_attributes")
            .HasColumnType("jsonb");

        builder.Property(b => b.BillIsActive)
            .HasColumnName("bill_is_active")
            .HasDefaultValue(true);

        builder.Property(b => b.BillCreatedAt)
            .HasColumnName("bill_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(b => b.BillUpdatedAt)
            .HasColumnName("bill_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for common queries
        builder.HasIndex(b => new { b.UserId, b.BillStatus });
        builder.HasIndex(b => new { b.UserId, b.BillDate });
        builder.HasIndex(b => new { b.UserId, b.BillDueDate });

        // Composite index for common filter combinations (status + date range)
        builder.HasIndex(b => new { b.UserId, b.BillStatus, b.BillDate })
            .HasDatabaseName("ix_bills_user_status_date");

        builder.HasIndex(b => new { b.UserId, b.BillCategory });
    }
}
