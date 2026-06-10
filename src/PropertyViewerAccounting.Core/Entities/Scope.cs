namespace PropertyViewerAccounting.Core.Entities;

public class Scope
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentScopeId { get; set; }
    public Guid? ScopeTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Path { get; set; } = string.Empty;
    public int Level { get; set; }
    public int SortOrder { get; set; }
    public string? Metadata { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Scope? ParentScope { get; set; }
    public ScopeType? ScopeType { get; set; }
    public ICollection<Scope> ChildScopes { get; set; } = new List<Scope>();
    public ICollection<Item> Items { get; set; } = new List<Item>();
    public ICollection<ItemScope> ItemScopes { get; set; } = new List<ItemScope>();
    public ICollection<ScopeLink> SourceLinks { get; set; } = new List<ScopeLink>();
    public ICollection<ScopeLink> TargetLinks { get; set; } = new List<ScopeLink>();
}
