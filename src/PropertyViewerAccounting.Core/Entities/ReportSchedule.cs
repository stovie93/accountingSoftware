using PropertyViewerAccounting.Core.Enums;

namespace PropertyViewerAccounting.Core.Entities;

public class ReportSchedule
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ReportDefinitionId { get; set; }
    public Guid ConnectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Schedule configuration
    public ScheduleFrequency Frequency { get; set; }
    public string? CronExpression { get; set; } // For custom schedules
    public TimeOnly TimeOfDay { get; set; } // When to run
    public int? DayOfWeek { get; set; } // 0-6 for weekly schedules
    public int? DayOfMonth { get; set; } // 1-31 for monthly schedules

    // Date range
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Export configuration
    public string ExportFormat { get; set; } = "csv"; // csv, xlsx
    public string? DestinationTable { get; set; } // For database imports
    public string? DestinationPath { get; set; } // For file-based imports

    // Status tracking
    public bool IsActive { get; set; } = true;
    public DateTime? LastRunAt { get; set; }
    public string? LastRunStatus { get; set; }
    public DateTime? NextRunAt { get; set; }

    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ReportDefinition ReportDefinition { get; set; } = null!;
    public ClientAccountingConnection Connection { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public ICollection<ReportScheduleRun> Runs { get; set; } = new List<ReportScheduleRun>();
}
