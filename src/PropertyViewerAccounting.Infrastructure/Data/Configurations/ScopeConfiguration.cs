using PropertyViewerAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PropertyViewerAccounting.Infrastructure.Data.Configurations;

public class ScopeConfiguration : IEntityTypeConfiguration<Scope>
{
    public void Configure(EntityTypeBuilder<Scope> builder)
    {
        builder.ToTable("scopes");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(s => s.ParentScopeId)
            .HasColumnName("parent_scope_id");

        builder.Property(s => s.ScopeTypeId)
            .HasColumnName("scope_type_id");

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Code)
            .HasColumnName("code")
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(s => new { s.UserId, s.Code }).IsUnique();

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(s => s.Path)
            .HasColumnName("path")
            .HasMaxLength(2000)
            .IsRequired();
        builder.HasIndex(s => new { s.UserId, s.Path });

        builder.Property(s => s.Level)
            .HasColumnName("level")
            .HasDefaultValue(0);

        builder.Property(s => s.SortOrder)
            .HasColumnName("sort_order")
            .HasDefaultValue(0);

        builder.Property(s => s.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        builder.Property(s => s.IsSystem)
            .HasColumnName("is_system")
            .HasDefaultValue(false);

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.ParentScope)
            .WithMany(s => s.ChildScopes)
            .HasForeignKey(s => s.ParentScopeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ScopeType)
            .WithMany(st => st.Scopes)
            .HasForeignKey(s => s.ScopeTypeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
