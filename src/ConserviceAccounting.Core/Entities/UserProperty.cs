namespace ConserviceAccounting.Core.Entities;

/// <summary>
/// Bridge table linking Users to Properties.
/// Represents which users have access to or manage which properties.
/// </summary>
public class UserProperty
{
    public Guid UserPropertyId { get; set; }
    public Guid UserId { get; set; }
    public Guid PropertyId { get; set; }
    public string? UserPropertyRole { get; set; } // e.g., "Manager", "Viewer", "Admin"
    public string? UserPropertyAccessLevel { get; set; }
    public bool UserPropertyIsPrimary { get; set; }
    public DateTime UserPropertyAssignedAt { get; set; }
    public DateTime? UserPropertyExpiresAt { get; set; }
    public string? UserPropertyNotes { get; set; }
    public bool UserPropertyIsActive { get; set; } = true;
    public DateTime UserPropertyCreatedAt { get; set; }
    public DateTime UserPropertyUpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Property Property { get; set; } = null!;
}
