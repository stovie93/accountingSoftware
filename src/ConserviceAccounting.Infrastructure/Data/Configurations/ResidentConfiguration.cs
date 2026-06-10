using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class ResidentConfiguration : IEntityTypeConfiguration<Resident>
{
    public void Configure(EntityTypeBuilder<Resident> builder)
    {
        builder.ToTable("residents");

        builder.HasKey(r => r.ResidentId);
        builder.Property(r => r.ResidentId).HasColumnName("resident_id");

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.ResidentExternalId)
            .HasColumnName("resident_external_id")
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(r => new { r.UserId, r.ResidentExternalId }).IsUnique();

        // Name fields
        builder.Property(r => r.ResidentFirstName)
            .HasColumnName("resident_first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.ResidentLastName)
            .HasColumnName("resident_last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.ResidentMiddleName)
            .HasColumnName("resident_middle_name")
            .HasMaxLength(100);

        // Contact fields
        builder.Property(r => r.ResidentEmail)
            .HasColumnName("resident_email")
            .HasMaxLength(255);

        builder.Property(r => r.ResidentPhone)
            .HasColumnName("resident_phone")
            .HasMaxLength(20);

        builder.Property(r => r.ResidentAlternatePhone)
            .HasColumnName("resident_alternate_phone")
            .HasMaxLength(20);

        // Unit/Lease fields
        builder.Property(r => r.ResidentUnitNumber)
            .HasColumnName("resident_unit_number")
            .HasMaxLength(50);

        builder.Property(r => r.ResidentLeaseStart)
            .HasColumnName("resident_lease_start");

        builder.Property(r => r.ResidentLeaseEnd)
            .HasColumnName("resident_lease_end");

        builder.Property(r => r.ResidentMonthlyRent)
            .HasColumnName("resident_monthly_rent")
            .HasPrecision(18, 2);

        // Address fields
        builder.Property(r => r.ResidentAddress)
            .HasColumnName("resident_address")
            .HasMaxLength(500);

        builder.Property(r => r.ResidentCity)
            .HasColumnName("resident_city")
            .HasMaxLength(100);

        builder.Property(r => r.ResidentState)
            .HasColumnName("resident_state")
            .HasMaxLength(50);

        builder.Property(r => r.ResidentZipCode)
            .HasColumnName("resident_zip_code")
            .HasMaxLength(20);

        // Status fields
        builder.Property(r => r.ResidentStatus)
            .HasColumnName("resident_status")
            .HasMaxLength(50)
            .HasDefaultValue("Active");

        builder.Property(r => r.ResidentType)
            .HasColumnName("resident_type")
            .HasMaxLength(50);

        // Attributes (JSONB)
        builder.Property(r => r.ResidentAttributes)
            .HasColumnName("resident_attributes")
            .HasColumnType("jsonb");

        // Batch tracking
        builder.Property(r => r.ResidentBatchId)
            .HasColumnName("resident_batch_id");

        // Audit fields
        builder.Property(r => r.ResidentIsActive)
            .HasColumnName("resident_is_active")
            .HasDefaultValue(true);

        builder.Property(r => r.ResidentCreatedAt)
            .HasColumnName("resident_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(r => r.ResidentUpdatedAt)
            .HasColumnName("resident_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // User relationship
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for common queries
        builder.HasIndex(r => new { r.UserId, r.ResidentStatus });
        builder.HasIndex(r => new { r.UserId, r.ResidentIsActive });
        builder.HasIndex(r => r.ResidentBatchId);
    }
}
