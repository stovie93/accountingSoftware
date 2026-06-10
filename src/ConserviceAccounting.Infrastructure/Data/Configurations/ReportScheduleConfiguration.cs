using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class ReportScheduleConfiguration : IEntityTypeConfiguration<ReportSchedule>
{
    public void Configure(EntityTypeBuilder<ReportSchedule> builder)
    {
        builder.ToTable("report_schedules");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(s => s.ReportDefinitionId)
            .HasColumnName("report_definition_id")
            .IsRequired();

        builder.Property(s => s.ConnectionId)
            .HasColumnName("connection_id")
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(s => s.Frequency)
            .HasColumnName("frequency")
            .HasConversion<int>();

        builder.Property(s => s.CronExpression)
            .HasColumnName("cron_expression")
            .HasMaxLength(100);

        builder.Property(s => s.TimeOfDay)
            .HasColumnName("time_of_day");

        builder.Property(s => s.DayOfWeek)
            .HasColumnName("day_of_week");

        builder.Property(s => s.DayOfMonth)
            .HasColumnName("day_of_month");

        builder.Property(s => s.StartDate)
            .HasColumnName("start_date");

        builder.Property(s => s.EndDate)
            .HasColumnName("end_date");

        builder.Property(s => s.ExportFormat)
            .HasColumnName("export_format")
            .HasMaxLength(20)
            .HasDefaultValue("csv");

        builder.Property(s => s.DestinationTable)
            .HasColumnName("destination_table")
            .HasMaxLength(200);

        builder.Property(s => s.DestinationPath)
            .HasColumnName("destination_path")
            .HasMaxLength(500);

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(s => s.LastRunAt)
            .HasColumnName("last_run_at");

        builder.Property(s => s.LastRunStatus)
            .HasColumnName("last_run_status")
            .HasMaxLength(50);

        builder.Property(s => s.NextRunAt)
            .HasColumnName("next_run_at");

        builder.Property(s => s.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired();

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

        builder.HasOne(s => s.ReportDefinition)
            .WithMany()
            .HasForeignKey(s => s.ReportDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Connection)
            .WithMany(c => c.ReportSchedules)
            .HasForeignKey(s => s.ConnectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.CreatedBy)
            .WithMany()
            .HasForeignKey(s => s.CreatedById)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => s.NextRunAt);
    }
}
