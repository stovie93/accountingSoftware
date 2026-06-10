namespace PropertyViewerAccounting.Core.Entities;

/// <summary>
/// Bridge table linking Properties to Residents.
/// Represents which residents live at which properties (apartment complexes).
/// </summary>
public class PropertyResident
{
    public Guid PropertyResidentId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid ResidentId { get; set; }

    // Relationship metadata
    public string? PropertyResidentUnitNumber { get; set; }
    public DateTime? PropertyResidentMoveInDate { get; set; }
    public DateTime? PropertyResidentMoveOutDate { get; set; }
    public string? PropertyResidentType { get; set; }
    public string? PropertyResidentNotes { get; set; }

    // Audit fields
    public bool PropertyResidentIsActive { get; set; } = true;
    public DateTime PropertyResidentCreatedAt { get; set; }
    public DateTime PropertyResidentUpdatedAt { get; set; }

    // Navigation properties
    public Property Property { get; set; } = null!;
    public Resident Resident { get; set; } = null!;
}
