using PropertyViewerAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PropertyViewerAccounting.Infrastructure.Data.Configurations;

public class AccountingSoftwareConfiguration : IEntityTypeConfiguration<AccountingSoftware>
{
    public void Configure(EntityTypeBuilder<AccountingSoftware> builder)
    {
        builder.ToTable("accounting_software");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(a => a.ConnectionType)
            .HasColumnName("connection_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(500);

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(a => a.Code).IsUnique();
    }
}
