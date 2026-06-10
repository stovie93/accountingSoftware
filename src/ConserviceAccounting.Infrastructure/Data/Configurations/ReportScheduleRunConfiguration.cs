using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class ReportScheduleRunConfiguration : IEntityTypeConfiguration<ReportScheduleRun>
{
    public void Configure(EntityTypeBuilder<ReportScheduleRun> builder)
    {
        builder.ToTable("report_schedule_runs");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.ScheduleId)
            .HasColumnName("schedule_id")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<int>();

        builder.Property(r => r.StartedAt)
            .HasColumnName("started_at");

        builder.Property(r => r.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(r => r.RecordsProcessed)
            .HasColumnName("records_processed")
            .HasDefaultValue(0);

        builder.Property(r => r.RecordsFailed)
            .HasColumnName("records_failed")
            .HasDefaultValue(0);

        builder.Property(r => r.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.Property(r => r.OutputFilePath)
            .HasColumnName("output_file_path")
            .HasMaxLength(500);

        builder.Property(r => r.FileSizeBytes)
            .HasColumnName("file_size_bytes");

        builder.HasOne(r => r.Schedule)
            .WithMany(s => s.Runs)
            .HasForeignKey(r => r.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.ScheduleId);
        builder.HasIndex(r => r.StartedAt);
    }
}
