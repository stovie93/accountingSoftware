namespace ConserviceAccounting.Core.Entities;

/// <summary>
/// A standalone object that can be linked to other entities and items.
/// Examples: Oakwood Apartments (Property), Denver Power (Provider), Electric (UtilityType)
/// </summary>
public class Entity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid EntityTypeId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public string? Attributes { get; set; } // JSONB for flexible custom fields
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public EntityType EntityType { get; set; } = null!;

    // Links to other entities (this entity as source)
    public ICollection<EntityLink> SourceLinks { get; set; } = new List<EntityLink>();
    // Links from other entities (this entity as target)
    public ICollection<EntityLink> TargetLinks { get; set; } = new List<EntityLink>();
    // Items linked to this entity
    public ICollection<ItemEntity> ItemEntities { get; set; } = new List<ItemEntity>();
    // Data pushes associated with this entity
    public ICollection<DataPush> DataPushes { get; set; } = new List<DataPush>();
}
