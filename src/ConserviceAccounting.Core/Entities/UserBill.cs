namespace ConserviceAccounting.Core.Entities;

/// <summary>
/// Bridge table linking Users to Bills.
/// Represents user responsibility, approval, or assignment to bills.
/// </summary>
public class UserBill
{
    public Guid UserBillId { get; set; }
    public Guid UserId { get; set; }
    public Guid BillId { get; set; }
    public string? UserBillRole { get; set; } // e.g., "Approver", "Reviewer", "Processor"
    public string? UserBillStatus { get; set; } // e.g., "Pending", "Approved", "Rejected"
    public DateTime UserBillAssignedAt { get; set; }
    public DateTime? UserBillActionAt { get; set; }
    public string? UserBillNotes { get; set; }
    public bool UserBillIsActive { get; set; } = true;
    public DateTime UserBillCreatedAt { get; set; }
    public DateTime UserBillUpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Bill Bill { get; set; } = null!;
}
