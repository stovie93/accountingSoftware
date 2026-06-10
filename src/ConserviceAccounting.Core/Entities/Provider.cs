namespace ConserviceAccounting.Core.Entities;

public class Provider
{
    public Guid ProviderId { get; set; }
    public Guid UserId { get; set; }
    public string ProviderAccountId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string? ProviderCode { get; set; }
    public string? ProviderDescription { get; set; }
    public string? ProviderType { get; set; }
    public string? ProviderAddress { get; set; }
    public string? ProviderCity { get; set; }
    public string? ProviderState { get; set; }
    public string? ProviderZipCode { get; set; }
    public string? ProviderCountry { get; set; }
    public string? ProviderPhone { get; set; }
    public string? ProviderEmail { get; set; }
    public string? ProviderWebsite { get; set; }
    public string? ProviderContactName { get; set; }
    public string? ProviderAccountNumber { get; set; }
    public string? ProviderAttributes { get; set; } // JSONB for flexible custom fields
    public bool ProviderIsActive { get; set; } = true;
    public DateTime ProviderCreatedAt { get; set; }
    public DateTime ProviderUpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;

    // Bridge table collections
    public ICollection<PropertyProvider> PropertyProviders { get; set; } = new List<PropertyProvider>();
    public ICollection<ProviderUtility> ProviderUtilities { get; set; } = new List<ProviderUtility>();
    public ICollection<BillProvider> BillProviders { get; set; } = new List<BillProvider>();
}
