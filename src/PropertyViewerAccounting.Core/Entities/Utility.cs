namespace PropertyViewerAccounting.Core.Entities;

public class Utility
{
    public Guid UtilityId { get; set; }
    public Guid UserId { get; set; }
    public string UtilityName { get; set; } = string.Empty;
    public string UtilityType { get; set; } = string.Empty;
    public string? UtilityCode { get; set; }
    public string? UtilityDescription { get; set; }
    public string? UtilityAttributes { get; set; } // JSONB for flexible custom fields
    public bool UtilityIsActive { get; set; } = true;
    public DateTime UtilityCreatedAt { get; set; }
    public DateTime UtilityUpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;

    // Bridge table collections
    public ICollection<PropertyUtility> PropertyUtilities { get; set; } = new List<PropertyUtility>();
    public ICollection<ProviderUtility> ProviderUtilities { get; set; } = new List<ProviderUtility>();
    public ICollection<BillUtility> BillUtilities { get; set; } = new List<BillUtility>();
}
