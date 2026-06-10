namespace PropertyViewerAccounting.Core.Entities;

public class ClientAccountingConnection
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AccountingSoftwareId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Database connection fields
    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
    public string? DatabaseServer { get; set; }
    public string? DatabaseUsername { get; set; }
    public string? DatabasePassword { get; set; }

    // API connection fields
    public string? ApiEndpoint { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }

    // SFTP connection fields
    public string? SftpHost { get; set; }
    public int? SftpPort { get; set; }
    public string? SftpUsername { get; set; }
    public string? SftpPassword { get; set; }
    public string? SftpPath { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? LastTestedAt { get; set; }
    public string? LastTestStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public AccountingSoftware AccountingSoftware { get; set; } = null!;
    public ICollection<ReportSchedule> ReportSchedules { get; set; } = new List<ReportSchedule>();
}
