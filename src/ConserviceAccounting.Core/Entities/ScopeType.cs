namespace ConserviceAccounting.Core.Entities;

/// <summary>
/// Categories for organizing scopes (e.g., Property, Utility Type, Vendor, Period)
/// </summary>
public class ScopeType
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public string? Color { get; set; } // For UI display
    public string? Icon { get; set; } // For UI display
    public int SortOrder { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Scope> Scopes { get; set; } = new List<Scope>();
}
