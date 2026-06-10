namespace ConserviceAccounting.Core.Entities;

/// <summary>
/// Bridge table linking Properties to Bills.
/// Represents which bills belong to which properties.
/// </summary>
public class PropertyBill
{
    public Guid PropertyBillId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid BillId { get; set; }
    public string? PropertyBillAllocationMethod { get; set; } // e.g., "Full", "Percentage", "Fixed"
    public decimal? PropertyBillAllocationPercent { get; set; }
    public decimal? PropertyBillAllocationAmount { get; set; }
    public string? PropertyBillCostCenter { get; set; }
    public string? PropertyBillGLCode { get; set; }
    public string? PropertyBillNotes { get; set; }
    public bool PropertyBillIsPrimary { get; set; }
    public bool PropertyBillIsActive { get; set; } = true;
    public DateTime PropertyBillCreatedAt { get; set; }
    public DateTime PropertyBillUpdatedAt { get; set; }

    // Navigation properties
    public Property Property { get; set; } = null!;
    public Bill Bill { get; set; } = null!;
}
