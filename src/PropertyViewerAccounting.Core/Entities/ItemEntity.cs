namespace PropertyViewerAccounting.Core.Entities;

/// <summary>
/// Bridge table connecting items (bills) to entities.
/// An item can be linked to multiple entities (e.g., a bill linked to Property + Provider + UtilityType)
/// </summary>
public class ItemEntity
{
    public Guid ItemId { get; set; }
    public Guid EntityId { get; set; }
    public DateTime AssignedAt { get; set; }

    public Item Item { get; set; } = null!;
    public Entity Entity { get; set; } = null!;
}
