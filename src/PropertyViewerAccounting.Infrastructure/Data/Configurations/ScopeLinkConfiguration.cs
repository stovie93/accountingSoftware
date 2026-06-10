using PropertyViewerAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PropertyViewerAccounting.Infrastructure.Data.Configurations;

public class ScopeLinkConfiguration : IEntityTypeConfiguration<ScopeLink>
{
    public void Configure(EntityTypeBuilder<ScopeLink> builder)
    {
        builder.ToTable("scope_links");

        builder.HasKey(sl => sl.Id);
        builder.Property(sl => sl.Id).HasColumnName("id");

        builder.Property(sl => sl.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(sl => sl.SourceScopeId)
            .HasColumnName("source_scope_id")
            .IsRequired();

        builder.Property(sl => sl.TargetScopeId)
            .HasColumnName("target_scope_id")
            .IsRequired();

        builder.Property(sl => sl.LinkType)
            .HasColumnName("link_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(sl => sl.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(sl => sl.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Unique constraint: only one link of each type between two scopes
        builder.HasIndex(sl => new { sl.UserId, sl.SourceScopeId, sl.TargetScopeId, sl.LinkType }).IsUnique();

        builder.HasOne(sl => sl.User)
            .WithMany()
            .HasForeignKey(sl => sl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sl => sl.SourceScope)
            .WithMany(s => s.SourceLinks)
            .HasForeignKey(sl => sl.SourceScopeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sl => sl.TargetScope)
            .WithMany(s => s.TargetLinks)
            .HasForeignKey(sl => sl.TargetScopeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
