namespace PropertyViewerAccounting.Core.Entities;

public class Property
{
    public Guid PropertyId { get; set; }
    public Guid UserId { get; set; }
    public string PropertyClientId { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string PropertyAddress { get; set; } = string.Empty;
    public string? PropertyCity { get; set; }
    public string? PropertyState { get; set; }
    public string? PropertyZipCode { get; set; }
    public string? PropertyCountry { get; set; }
    public string PropertyType { get; set; } = string.Empty;
    public string? PropertyCode { get; set; }
    public string? PropertyDescription { get; set; }
    public int? PropertyUnitCount { get; set; }
    public decimal? PropertySquareFootage { get; set; }
    public bool PropertyIsActive { get; set; } = true;
    public DateTime PropertyCreatedAt { get; set; }
    public DateTime PropertyUpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;

    // Bridge table collections
    public ICollection<UserProperty> UserProperties { get; set; } = new List<UserProperty>();
    public ICollection<PropertyBill> PropertyBills { get; set; } = new List<PropertyBill>();
    public ICollection<PropertyUtility> PropertyUtilities { get; set; } = new List<PropertyUtility>();
    public ICollection<PropertyProvider> PropertyProviders { get; set; } = new List<PropertyProvider>();
    public ICollection<PropertyResident> PropertyResidents { get; set; } = new List<PropertyResident>();
}
