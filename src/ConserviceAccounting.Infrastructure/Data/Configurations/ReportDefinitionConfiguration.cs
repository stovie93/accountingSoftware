using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class ReportDefinitionConfiguration : IEntityTypeConfiguration<ReportDefinition>
{
    public void Configure(EntityTypeBuilder<ReportDefinition> builder)
    {
        builder.ToTable("report_definitions");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Type)
            .HasColumnName("type")
            .HasConversion<int>();

        builder.Property(r => r.Configuration)
            .HasColumnName("configuration")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(r => r.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired();

        builder.Property(r => r.IsShared)
            .HasColumnName("is_shared")
            .HasDefaultValue(false);

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.CreatedBy)
            .WithMany(u => u.CreatedReports)
            .HasForeignKey(r => r.CreatedById)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
