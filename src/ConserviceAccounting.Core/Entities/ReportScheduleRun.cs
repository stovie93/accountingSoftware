using ConserviceAccounting.Core.Enums;

namespace ConserviceAccounting.Core.Entities;

public class ReportScheduleRun
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public ScheduleRunStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int RecordsProcessed { get; set; }
    public int RecordsFailed { get; set; }
    public string? ErrorMessage { get; set; }
    public string? OutputFilePath { get; set; }
    public long? FileSizeBytes { get; set; }

    public ReportSchedule Schedule { get; set; } = null!;
}
