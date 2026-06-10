using PropertyViewerAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PropertyViewerAccounting.Infrastructure.Data.Configurations;

public class DataPushConfiguration : IEntityTypeConfiguration<DataPush>
{
    public void Configure(EntityTypeBuilder<DataPush> builder)
    {
        builder.ToTable("data_pushes");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(d => d.EntityId)
            .HasColumnName("entity_id")
            .IsRequired();

        builder.Property(d => d.BatchId)
            .HasColumnName("batch_id")
            .IsRequired();
        builder.HasIndex(d => d.BatchId).IsUnique();

        builder.Property(d => d.PushedAt)
            .HasColumnName("pushed_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(d => d.RecordCount)
            .HasColumnName("record_count")
            .HasDefaultValue(0);

        builder.Property(d => d.Status)
            .HasColumnName("status")
            .HasConversion<int>();

        builder.Property(d => d.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Entity)
            .WithMany(e => e.DataPushes)
            .HasForeignKey(d => d.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
