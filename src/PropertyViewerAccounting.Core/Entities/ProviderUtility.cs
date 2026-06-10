namespace PropertyViewerAccounting.Core.Entities;

/// <summary>
/// Bridge table linking Providers to Utilities.
/// Represents which utility types each provider offers.
/// </summary>
public class ProviderUtility
{
    public Guid ProviderUtilityId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid UtilityId { get; set; }
    public string? ProviderUtilityServiceArea { get; set; }
    public decimal? ProviderUtilityDefaultRate { get; set; }
    public string? ProviderUtilityRateUnit { get; set; }
    public string? ProviderUtilityNotes { get; set; }
    public bool ProviderUtilityIsPrimary { get; set; }
    public bool ProviderUtilityIsActive { get; set; } = true;
    public DateTime ProviderUtilityCreatedAt { get; set; }
    public DateTime ProviderUtilityUpdatedAt { get; set; }

    // Navigation properties
    public Provider Provider { get; set; } = null!;
    public Utility Utility { get; set; } = null!;
}
