using PropertyViewerAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PropertyViewerAccounting.Infrastructure.Data.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("properties");

        builder.HasKey(p => p.PropertyId);
        builder.Property(p => p.PropertyId).HasColumnName("property_id");

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.PropertyClientId)
            .HasColumnName("property_client_id")
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(p => new { p.UserId, p.PropertyClientId }).IsUnique();

        builder.Property(p => p.PropertyName)
            .HasColumnName("property_name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(p => p.PropertyAddress)
            .HasColumnName("property_address")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.PropertyCity)
            .HasColumnName("property_city")
            .HasMaxLength(100);

        builder.Property(p => p.PropertyState)
            .HasColumnName("property_state")
            .HasMaxLength(100);

        builder.Property(p => p.PropertyZipCode)
            .HasColumnName("property_zip_code")
            .HasMaxLength(20);

        builder.Property(p => p.PropertyCountry)
            .HasColumnName("property_country")
            .HasMaxLength(100);

        builder.Property(p => p.PropertyType)
            .HasColumnName("property_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.PropertyCode)
            .HasColumnName("property_code")
            .HasMaxLength(50);

        builder.Property(p => p.PropertyDescription)
            .HasColumnName("property_description")
            .HasMaxLength(2000);

        builder.Property(p => p.PropertyUnitCount)
            .HasColumnName("property_unit_count");

        builder.Property(p => p.PropertySquareFootage)
            .HasColumnName("property_square_footage")
            .HasPrecision(18, 2);

        builder.Property(p => p.PropertyIsActive)
            .HasColumnName("property_is_active")
            .HasDefaultValue(true);

        builder.Property(p => p.PropertyCreatedAt)
            .HasColumnName("property_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.PropertyUpdatedAt)
            .HasColumnName("property_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for common queries
        builder.HasIndex(p => new { p.UserId, p.PropertyType });
        builder.HasIndex(p => new { p.UserId, p.PropertyIsActive });
    }
}
