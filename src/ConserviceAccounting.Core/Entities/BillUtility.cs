namespace ConserviceAccounting.Core.Entities;

/// <summary>
/// Bridge table linking Bills to Utilities.
/// Represents which utility types are associated with each bill.
/// </summary>
public class BillUtility
{
    public Guid BillUtilityId { get; set; }
    public Guid BillId { get; set; }
    public Guid UtilityId { get; set; }
    public decimal? BillUtilityAmount { get; set; }
    public decimal? BillUtilityQuantity { get; set; }
    public string? BillUtilityUnit { get; set; }
    public decimal? BillUtilityRate { get; set; }
    public string? BillUtilityNotes { get; set; }
    public bool BillUtilityIsPrimary { get; set; }
    public bool BillUtilityIsActive { get; set; } = true;
    public DateTime BillUtilityCreatedAt { get; set; }
    public DateTime BillUtilityUpdatedAt { get; set; }

    // Navigation properties
    public Bill Bill { get; set; } = null!;
    public Utility Utility { get; set; } = null!;
}
