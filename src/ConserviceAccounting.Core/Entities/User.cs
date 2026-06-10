using ConserviceAccounting.Core.Enums;

namespace ConserviceAccounting.Core.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserPasswordHash { get; set; } = string.Empty;
    public string? UserFirstName { get; set; }
    public string? UserLastName { get; set; }
    public string? UserTitle { get; set; }
    public string? UserGroup { get; set; }
    public string? UserPhone { get; set; }
    public string? UserDepartment { get; set; }
    public UserRole UserRole { get; set; }
    public bool UserIsActive { get; set; } = true;
    public DateTime UserCreatedAt { get; set; }
    public DateTime UserUpdatedAt { get; set; }
    public DateTime? UserLastLoginAt { get; set; }

    // Navigation properties
    public ICollection<ReportDefinition> CreatedReports { get; set; } = new List<ReportDefinition>();

    // Bridge table collections
    public ICollection<UserProperty> UserProperties { get; set; } = new List<UserProperty>();
    public ICollection<UserBill> UserBills { get; set; } = new List<UserBill>();
}
