using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class UserBillConfiguration : IEntityTypeConfiguration<UserBill>
{
    public void Configure(EntityTypeBuilder<UserBill> builder)
    {
        builder.ToTable("user_bills");

        builder.HasKey(ub => ub.UserBillId);
        builder.Property(ub => ub.UserBillId).HasColumnName("user_bill_id");

        builder.Property(ub => ub.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ub => ub.BillId)
            .HasColumnName("bill_id")
            .IsRequired();

        builder.HasIndex(ub => new { ub.UserId, ub.BillId, ub.UserBillRole }).IsUnique();

        builder.Property(ub => ub.UserBillRole)
            .HasColumnName("user_bill_role")
            .HasMaxLength(100);

        builder.Property(ub => ub.UserBillStatus)
            .HasColumnName("user_bill_status")
            .HasMaxLength(50);

        builder.Property(ub => ub.UserBillAssignedAt)
            .HasColumnName("user_bill_assigned_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ub => ub.UserBillActionAt)
            .HasColumnName("user_bill_action_at");

        builder.Property(ub => ub.UserBillNotes)
            .HasColumnName("user_bill_notes")
            .HasMaxLength(2000);

        builder.Property(ub => ub.UserBillIsActive)
            .HasColumnName("user_bill_is_active")
            .HasDefaultValue(true);

        builder.Property(ub => ub.UserBillCreatedAt)
            .HasColumnName("user_bill_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ub => ub.UserBillUpdatedAt)
            .HasColumnName("user_bill_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(ub => ub.User)
            .WithMany(u => u.UserBills)
            .HasForeignKey(ub => ub.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ub => ub.Bill)
            .WithMany(b => b.UserBills)
            .HasForeignKey(ub => ub.BillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
