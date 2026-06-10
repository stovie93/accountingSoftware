namespace PropertyViewerAccounting.Core.Entities;

/// <summary>
/// Junction table for many-to-many relationship between Items and Scopes.
/// An item can belong to multiple scopes (e.g., a bill tagged with both "Oakwood" and "Electric")
/// </summary>
public class ItemScope
{
    public Guid ItemId { get; set; }
    public Guid ScopeId { get; set; }
    public DateTime AssignedAt { get; set; }

    public Item Item { get; set; } = null!;
    public Scope Scope { get; set; } = null!;
}
