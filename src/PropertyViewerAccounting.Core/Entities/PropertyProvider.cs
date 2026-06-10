namespace PropertyViewerAccounting.Core.Entities;

/// <summary>
/// Bridge table linking Properties to Providers.
/// Represents which providers service which properties.
/// </summary>
public class PropertyProvider
{
    public Guid PropertyProviderId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid ProviderId { get; set; }
    public string? PropertyProviderAccountNumber { get; set; }
    public string? PropertyProviderServiceType { get; set; }
    public DateTime? PropertyProviderStartDate { get; set; }
    public DateTime? PropertyProviderEndDate { get; set; }
    public string? PropertyProviderContractNumber { get; set; }
    public string? PropertyProviderNotes { get; set; }
    public bool PropertyProviderIsPrimary { get; set; }
    public bool PropertyProviderIsActive { get; set; } = true;
    public DateTime PropertyProviderCreatedAt { get; set; }
    public DateTime PropertyProviderUpdatedAt { get; set; }

    // Navigation properties
    public Property Property { get; set; } = null!;
    public Provider Provider { get; set; } = null!;
}
