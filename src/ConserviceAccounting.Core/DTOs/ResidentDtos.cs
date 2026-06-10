namespace ConserviceAccounting.Core.DTOs;

// Resident DTOs
public record ResidentDto(
    Guid ResidentId,
    string ResidentExternalId,
    string ResidentFirstName,
    string ResidentLastName,
    string? ResidentMiddleName,
    string? ResidentEmail,
    string? ResidentPhone,
    string? ResidentAlternatePhone,
    string? ResidentUnitNumber,
    DateTime? ResidentLeaseStart,
    DateTime? ResidentLeaseEnd,
    decimal? ResidentMonthlyRent,
    string? ResidentAddress,
    string? ResidentCity,
    string? ResidentState,
    string? ResidentZipCode,
    string ResidentStatus,
    string? ResidentType,
    Dictionary<string, object>? ResidentAttributes,
    Guid? ResidentBatchId,
    bool ResidentIsActive,
    DateTime ResidentCreatedAt,
    DateTime ResidentUpdatedAt
);

public record CreateResidentRequest(
    string ResidentExternalId,
    string ResidentFirstName,
    string ResidentLastName,
    string? ResidentMiddleName,
    string? ResidentEmail,
    string? ResidentPhone,
    string? ResidentAlternatePhone,
    string? ResidentUnitNumber,
    DateTime? ResidentLeaseStart,
    DateTime? ResidentLeaseEnd,
    decimal? ResidentMonthlyRent,
    string? ResidentAddress,
    string? ResidentCity,
    string? ResidentState,
    string? ResidentZipCode,
    string? ResidentStatus,
    string? ResidentType,
    Dictionary<string, object>? ResidentAttributes,
    Guid? PropertyId
);

public record UpdateResidentRequest(
    string ResidentFirstName,
    string ResidentLastName,
    string? ResidentMiddleName,
    string? ResidentEmail,
    string? ResidentPhone,
    string? ResidentAlternatePhone,
    string? ResidentUnitNumber,
    DateTime? ResidentLeaseStart,
    DateTime? ResidentLeaseEnd,
    decimal? ResidentMonthlyRent,
    string? ResidentAddress,
    string? ResidentCity,
    string? ResidentState,
    string? ResidentZipCode,
    string? ResidentStatus,
    string? ResidentType,
    Dictionary<string, object>? ResidentAttributes,
    bool ResidentIsActive
);

public record ResidentListResponse(
    List<ResidentDto> Residents,
    int TotalCount,
    int Page,
    int PageSize
);

// Bulk Import DTOs
public record BulkResidentInput(
    string ExternalId,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? Email,
    string? Phone,
    string? AlternatePhone,
    string? UnitNumber,
    DateTime? LeaseStart,
    DateTime? LeaseEnd,
    decimal? MonthlyRent,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    string? Status,
    string? Type,
    Dictionary<string, object>? Attributes
);

public record BulkImportResidentsRequest(
    Guid PropertyId,
    List<BulkResidentInput> Residents,
    bool UpdateExisting = true
);

public record ResidentImportResponse
{
    public Guid BatchId { get; set; }
    public Guid PropertyId { get; set; }
    public int TotalRequested { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Failed { get; set; }
    public List<ResidentImportError> Errors { get; set; } = new();
}

public record ResidentImportError
{
    public int Index { get; set; }
    public string? ExternalId { get; set; }
    public string Message { get; set; } = string.Empty;
}

// Push to Conservice DTOs
public record ResidentPushRequest(
    Guid PropertyId,
    List<Guid>? ResidentIds
);

public record ResidentPushResponse
{
    public Guid PushId { get; set; }
    public Guid PropertyId { get; set; }
    public int ResidentCount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime PushedAt { get; set; }
}

// Relationship DTOs
public record ResidentRelatedPropertyDto(
    Guid PropertyId,
    string PropertyName,
    string? PropertyAddress,
    string? UnitNumber,
    DateTime? MoveInDate,
    DateTime? MoveOutDate
);

public record ResidentRelationshipsDto(
    List<ResidentRelatedPropertyDto> Properties
);

public record PropertyRelatedResidentDto(
    Guid ResidentId,
    string ResidentExternalId,
    string ResidentFirstName,
    string ResidentLastName,
    string? ResidentEmail,
    string? UnitNumber,
    string ResidentStatus
);
