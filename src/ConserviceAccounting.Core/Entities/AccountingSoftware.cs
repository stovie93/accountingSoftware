namespace ConserviceAccounting.Core.Entities;

public class AccountingSoftware
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ConnectionType { get; set; } = "Database"; // Database, API, SFTP, File
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ClientAccountingConnection> Connections { get; set; } = new List<ClientAccountingConnection>();
}
