namespace ConserviceAccounting.Core.Entities;

/// <summary>
/// Categories for organizing entities (e.g., Property, Provider, UtilityType, PropertyManager)
/// </summary>
public class EntityType
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Entity> Entities { get; set; } = new List<Entity>();
}
