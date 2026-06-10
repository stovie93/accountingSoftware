namespace PropertyViewerAccounting.Core.Entities;

/// <summary>
/// Bridge table linking Bills to Providers.
/// Represents which provider issued each bill.
/// </summary>
public class BillProvider
{
    public Guid BillProviderId { get; set; }
    public Guid BillId { get; set; }
    public Guid ProviderId { get; set; }
    public string? BillProviderAccountNumber { get; set; }
    public string? BillProviderInvoiceNumber { get; set; }
    public string? BillProviderNotes { get; set; }
    public bool BillProviderIsPrimary { get; set; }
    public bool BillProviderIsActive { get; set; } = true;
    public DateTime BillProviderCreatedAt { get; set; }
    public DateTime BillProviderUpdatedAt { get; set; }

    // Navigation properties
    public Bill Bill { get; set; } = null!;
    public Provider Provider { get; set; } = null!;
}
