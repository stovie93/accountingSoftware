namespace PropertyViewerAccounting.Core.Entities;

/// <summary>
/// Bridge table linking PropertyUtility to Bills.
/// This is a 3-way bridge: Property + Utility combination linked to Bills.
/// Allows tracking bills for specific utility services at specific properties.
/// </summary>
public class PropertyUtilityBill
{
    public Guid PropertyUtilityBillId { get; set; }
    public Guid PropertyUtilityId { get; set; }
    public Guid BillId { get; set; }
    public decimal? PropertyUtilityBillAmount { get; set; }
    public decimal? PropertyUtilityBillQuantity { get; set; }
    public string? PropertyUtilityBillPeriod { get; set; }
    public string? PropertyUtilityBillNotes { get; set; }
    public bool PropertyUtilityBillIsActive { get; set; } = true;
    public DateTime PropertyUtilityBillCreatedAt { get; set; }
    public DateTime PropertyUtilityBillUpdatedAt { get; set; }

    // Navigation properties
    public PropertyUtility PropertyUtility { get; set; } = null!;
    public Bill Bill { get; set; } = null!;
}
