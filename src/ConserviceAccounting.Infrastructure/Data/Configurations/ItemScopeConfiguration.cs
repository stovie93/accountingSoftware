using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class ItemScopeConfiguration : IEntityTypeConfiguration<ItemScope>
{
    public void Configure(EntityTypeBuilder<ItemScope> builder)
    {
        builder.ToTable("item_scopes");

        // Composite primary key
        builder.HasKey(iss => new { iss.ItemId, iss.ScopeId });

        builder.Property(iss => iss.ItemId)
            .HasColumnName("item_id");

        builder.Property(iss => iss.ScopeId)
            .HasColumnName("scope_id");

        builder.Property(iss => iss.AssignedAt)
            .HasColumnName("assigned_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes for efficient querying
        builder.HasIndex(iss => iss.ItemId);
        builder.HasIndex(iss => iss.ScopeId);

        builder.HasOne(iss => iss.Item)
            .WithMany()
            .HasForeignKey(iss => iss.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(iss => iss.Scope)
            .WithMany(s => s.ItemScopes)
            .HasForeignKey(iss => iss.ScopeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
