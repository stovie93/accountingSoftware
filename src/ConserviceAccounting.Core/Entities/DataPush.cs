using ConserviceAccounting.Core.Enums;

namespace ConserviceAccounting.Core.Entities;

public class DataPush
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid EntityId { get; set; }
    public Guid BatchId { get; set; }
    public DateTime PushedAt { get; set; }
    public int RecordCount { get; set; }
    public DataPushStatus Status { get; set; }
    public string? ErrorMessage { get; set; }

    public User User { get; set; } = null!;
    public Entity Entity { get; set; } = null!;
}
