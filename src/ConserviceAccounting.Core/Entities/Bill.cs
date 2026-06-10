namespace ConserviceAccounting.Core.Entities;

public class Bill
{
    public Guid BillId { get; set; }
    public Guid UserId { get; set; }
    public string BillReferenceId { get; set; } = string.Empty;
    public decimal BillAmount { get; set; }
    public string BillStatus { get; set; } = string.Empty;
    public string? BillDescription { get; set; }
    public DateTime? BillDate { get; set; }
    public DateTime? BillDueDate { get; set; }
    public DateTime? BillPaidDate { get; set; }
    public string? BillPeriodStart { get; set; }
    public string? BillPeriodEnd { get; set; }
    public decimal? BillQuantity { get; set; }
    public string? BillUnit { get; set; }
    public decimal? BillRate { get; set; }
    public decimal? BillTax { get; set; }
    public decimal? BillTotalAmount { get; set; }
    public string? BillCurrency { get; set; }
    public string? BillNotes { get; set; }
    public string? BillCategory { get; set; }
    public string? BillExternalId { get; set; }
    public string? BillAttributes { get; set; } // JSONB for flexible custom fields
    public bool BillIsActive { get; set; } = true;
    public DateTime BillCreatedAt { get; set; }
    public DateTime BillUpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;

    // Bridge table collections
    public ICollection<UserBill> UserBills { get; set; } = new List<UserBill>();
    public ICollection<PropertyBill> PropertyBills { get; set; } = new List<PropertyBill>();
    public ICollection<BillUtility> BillUtilities { get; set; } = new List<BillUtility>();
    public ICollection<BillProvider> BillProviders { get; set; } = new List<BillProvider>();
}
