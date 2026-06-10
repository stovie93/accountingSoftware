using ConserviceAccounting.Core.Enums;

namespace ConserviceAccounting.Core.Entities;

public class ReportDefinition
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public string Configuration { get; set; } = "{}";
    public Guid CreatedById { get; set; }
    public bool IsShared { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
}
