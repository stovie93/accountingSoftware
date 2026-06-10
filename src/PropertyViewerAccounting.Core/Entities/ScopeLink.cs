namespace PropertyViewerAccounting.Core.Entities;

/// <summary>
/// Defines relationships between scopes without strict hierarchy.
/// Examples: "Oakwood is managed by Sunrise PM", "Electric is a utility for Oakwood"
/// </summary>
public class ScopeLink
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SourceScopeId { get; set; }
    public Guid TargetScopeId { get; set; }
    public string LinkType { get; set; } = null!; // e.g., "manages", "provides", "contains"
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Scope SourceScope { get; set; } = null!;
    public Scope TargetScope { get; set; } = null!;
}
