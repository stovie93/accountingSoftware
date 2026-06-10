namespace ConserviceAccounting.Core.Entities;

/// <summary>
/// Bridge table linking Properties to Utilities.
/// Represents which utility services are available at which properties.
/// </summary>
public class PropertyUtility
{
    public Guid PropertyUtilityId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid UtilityId { get; set; }
    public string? PropertyUtilityAccountNumber { get; set; }
    public string? PropertyUtilityMeterNumber { get; set; }
    public string? PropertyUtilityServiceAddress { get; set; }
    public DateTime? PropertyUtilityStartDate { get; set; }
    public DateTime? PropertyUtilityEndDate { get; set; }
    public decimal? PropertyUtilityBudget { get; set; }
    public string? PropertyUtilityNotes { get; set; }
    public bool PropertyUtilityIsActive { get; set; } = true;
    public DateTime PropertyUtilityCreatedAt { get; set; }
    public DateTime PropertyUtilityUpdatedAt { get; set; }

    // Navigation properties
    public Property Property { get; set; } = null!;
    public Utility Utility { get; set; } = null!;

    // Bridge to bills through this property-utility combination
    public ICollection<PropertyUtilityBill> PropertyUtilityBills { get; set; } = new List<PropertyUtilityBill>();
}
