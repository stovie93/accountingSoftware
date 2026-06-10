using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class ScopeTypeConfiguration : IEntityTypeConfiguration<ScopeType>
{
    public void Configure(EntityTypeBuilder<ScopeType> builder)
    {
        builder.ToTable("scope_types");

        builder.HasKey(st => st.Id);
        builder.Property(st => st.Id).HasColumnName("id");

        builder.Property(st => st.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(st => st.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(st => st.Code)
            .HasColumnName("code")
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(st => new { st.UserId, st.Code }).IsUnique();

        builder.Property(st => st.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(st => st.Color)
            .HasColumnName("color")
            .HasMaxLength(50);

        builder.Property(st => st.Icon)
            .HasColumnName("icon")
            .HasMaxLength(100);

        builder.Property(st => st.SortOrder)
            .HasColumnName("sort_order")
            .HasDefaultValue(0);

        builder.Property(st => st.IsSystem)
            .HasColumnName("is_system")
            .HasDefaultValue(false);

        builder.Property(st => st.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(st => st.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(st => st.User)
            .WithMany()
            .HasForeignKey(st => st.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
