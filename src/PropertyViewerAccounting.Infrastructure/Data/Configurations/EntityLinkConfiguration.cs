using PropertyViewerAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PropertyViewerAccounting.Infrastructure.Data.Configurations;

public class EntityLinkConfiguration : IEntityTypeConfiguration<EntityLink>
{
    public void Configure(EntityTypeBuilder<EntityLink> builder)
    {
        builder.ToTable("entity_links");

        builder.HasKey(el => el.Id);
        builder.Property(el => el.Id).HasColumnName("id");

        builder.Property(el => el.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(el => el.SourceEntityId)
            .HasColumnName("source_entity_id")
            .IsRequired();

        builder.Property(el => el.TargetEntityId)
            .HasColumnName("target_entity_id")
            .IsRequired();

        builder.Property(el => el.LinkType)
            .HasColumnName("link_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(el => el.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(el => el.Attributes)
            .HasColumnName("attributes")
            .HasColumnType("jsonb");

        builder.Property(el => el.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Unique constraint: only one link of each type between two entities
        builder.HasIndex(el => new { el.UserId, el.SourceEntityId, el.TargetEntityId, el.LinkType }).IsUnique();

        // Indexes for efficient querying
        builder.HasIndex(el => new { el.UserId, el.SourceEntityId });
        builder.HasIndex(el => new { el.UserId, el.TargetEntityId });
        builder.HasIndex(el => new { el.UserId, el.LinkType });

        builder.HasOne(el => el.User)
            .WithMany()
            .HasForeignKey(el => el.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(el => el.SourceEntity)
            .WithMany(e => e.SourceLinks)
            .HasForeignKey(el => el.SourceEntityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(el => el.TargetEntity)
            .WithMany(e => e.TargetLinks)
            .HasForeignKey(el => el.TargetEntityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
