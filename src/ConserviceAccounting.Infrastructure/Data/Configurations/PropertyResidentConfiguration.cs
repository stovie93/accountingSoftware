using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class PropertyResidentConfiguration : IEntityTypeConfiguration<PropertyResident>
{
    public void Configure(EntityTypeBuilder<PropertyResident> builder)
    {
        builder.ToTable("property_residents");

        builder.HasKey(pr => pr.PropertyResidentId);
        builder.Property(pr => pr.PropertyResidentId).HasColumnName("property_resident_id");

        builder.Property(pr => pr.PropertyId)
            .HasColumnName("property_id")
            .IsRequired();

        builder.Property(pr => pr.ResidentId)
            .HasColumnName("resident_id")
            .IsRequired();

        builder.HasIndex(pr => new { pr.PropertyId, pr.ResidentId }).IsUnique();

        builder.Property(pr => pr.PropertyResidentUnitNumber)
            .HasColumnName("property_resident_unit_number")
            .HasMaxLength(50);

        builder.Property(pr => pr.PropertyResidentMoveInDate)
            .HasColumnName("property_resident_move_in_date");

        builder.Property(pr => pr.PropertyResidentMoveOutDate)
            .HasColumnName("property_resident_move_out_date");

        builder.Property(pr => pr.PropertyResidentType)
            .HasColumnName("property_resident_type")
            .HasMaxLength(50);

        builder.Property(pr => pr.PropertyResidentNotes)
            .HasColumnName("property_resident_notes")
            .HasMaxLength(2000);

        builder.Property(pr => pr.PropertyResidentIsActive)
            .HasColumnName("property_resident_is_active")
            .HasDefaultValue(true);

        builder.Property(pr => pr.PropertyResidentCreatedAt)
            .HasColumnName("property_resident_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(pr => pr.PropertyResidentUpdatedAt)
            .HasColumnName("property_resident_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(pr => pr.Property)
            .WithMany(p => p.PropertyResidents)
            .HasForeignKey(pr => pr.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pr => pr.Resident)
            .WithMany(r => r.PropertyResidents)
            .HasForeignKey(pr => pr.ResidentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
