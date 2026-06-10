namespace PropertyViewerAccounting.Core.Entities;

public class Resident
{
    public Guid ResidentId { get; set; }
    public Guid UserId { get; set; }

    // Core identification
    public string ResidentExternalId { get; set; } = string.Empty;
    public string ResidentFirstName { get; set; } = string.Empty;
    public string ResidentLastName { get; set; } = string.Empty;
    public string? ResidentMiddleName { get; set; }

    // Contact info
    public string? ResidentEmail { get; set; }
    public string? ResidentPhone { get; set; }
    public string? ResidentAlternatePhone { get; set; }

    // Unit/Lease info
    public string? ResidentUnitNumber { get; set; }
    public DateTime? ResidentLeaseStart { get; set; }
    public DateTime? ResidentLeaseEnd { get; set; }
    public decimal? ResidentMonthlyRent { get; set; }

    // Address (for mailing/billing purposes)
    public string? ResidentAddress { get; set; }
    public string? ResidentCity { get; set; }
    public string? ResidentState { get; set; }
    public string? ResidentZipCode { get; set; }

    // Status
    public string ResidentStatus { get; set; } = "Active";
    public string? ResidentType { get; set; }

    // Flexible fields (JSONB for extra data)
    public string? ResidentAttributes { get; set; }

    // Import tracking
    public Guid? ResidentBatchId { get; set; }

    // Audit fields
    public bool ResidentIsActive { get; set; } = true;
    public DateTime ResidentCreatedAt { get; set; }
    public DateTime ResidentUpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;

    // Bridge table collections
    public ICollection<PropertyResident> PropertyResidents { get; set; } = new List<PropertyResident>();
}
