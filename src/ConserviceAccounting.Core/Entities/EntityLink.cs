namespace ConserviceAccounting.Core.Entities;

/// <summary>
/// Bridge table connecting entities to each other.
/// Examples: "PropertyManager manages Property", "Provider serves Property"
/// </summary>
public class EntityLink
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SourceEntityId { get; set; }
    public Guid TargetEntityId { get; set; }
    public string LinkType { get; set; } = null!; // e.g., "manages", "serves", "provides"
    public string? Description { get; set; }
    public string? Attributes { get; set; } // JSONB for link-specific data
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Entity SourceEntity { get; set; } = null!;
    public Entity TargetEntity { get; set; } = null!;
}
