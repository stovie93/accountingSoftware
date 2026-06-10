namespace ConserviceAccounting.Core.DTOs;

// Property DTOs
public record PropertyDto(
    Guid PropertyId,
    string PropertyClientId,
    string PropertyName,
    string PropertyAddress,
    string? PropertyCity,
    string? PropertyState,
    string? PropertyZipCode,
    string? PropertyCountry,
    string PropertyType,
    string? PropertyCode,
    string? PropertyDescription,
    int? PropertyUnitCount,
    decimal? PropertySquareFootage,
    bool PropertyIsActive,
    DateTime PropertyCreatedAt,
    DateTime PropertyUpdatedAt
);

public record CreatePropertyRequest(
    string PropertyClientId,
    string PropertyName,
    string PropertyAddress,
    string? PropertyCity,
    string? PropertyState,
    string? PropertyZipCode,
    string? PropertyCountry,
    string PropertyType,
    string? PropertyCode,
    string? PropertyDescription,
    int? PropertyUnitCount,
    decimal? PropertySquareFootage
);

public record UpdatePropertyRequest(
    string PropertyName,
    string PropertyAddress,
    string? PropertyCity,
    string? PropertyState,
    string? PropertyZipCode,
    string? PropertyCountry,
    string PropertyType,
    string? PropertyCode,
    string? PropertyDescription,
    int? PropertyUnitCount,
    decimal? PropertySquareFootage,
    bool PropertyIsActive
);

public record PropertyListResponse(
    List<PropertyDto> Properties,
    int TotalCount,
    int Page,
    int PageSize
);

// Bill DTOs
public record BillDto(
    Guid BillId,
    string BillReferenceId,
    decimal BillAmount,
    string BillStatus,
    string? BillDescription,
    DateTime? BillDate,
    DateTime? BillDueDate,
    DateTime? BillPaidDate,
    string? BillPeriodStart,
    string? BillPeriodEnd,
    decimal? BillQuantity,
    string? BillUnit,
    decimal? BillRate,
    decimal? BillTax,
    decimal? BillTotalAmount,
    string? BillCurrency,
    string? BillNotes,
    string? BillCategory,
    string? BillExternalId,
    bool BillIsActive,
    DateTime BillCreatedAt,
    DateTime BillUpdatedAt,
    string? UtilityName,
    string? ProviderName
);

public record CreateBillRequest(
    string BillReferenceId,
    decimal BillAmount,
    string BillStatus,
    string? BillDescription,
    DateTime? BillDate,
    DateTime? BillDueDate,
    string? BillPeriodStart,
    string? BillPeriodEnd,
    decimal? BillQuantity,
    string? BillUnit,
    decimal? BillRate,
    decimal? BillTax,
    decimal? BillTotalAmount,
    string? BillCurrency,
    string? BillNotes,
    string? BillExternalId
);

public record UpdateBillRequest(
    decimal BillAmount,
    string BillStatus,
    string? BillDescription,
    DateTime? BillDate,
    DateTime? BillDueDate,
    DateTime? BillPaidDate,
    string? BillPeriodStart,
    string? BillPeriodEnd,
    decimal? BillQuantity,
    string? BillUnit,
    decimal? BillRate,
    decimal? BillTax,
    decimal? BillTotalAmount,
    string? BillCurrency,
    string? BillNotes,
    string? BillExternalId,
    bool BillIsActive
);

public record BillListResponse(
    List<BillDto> Bills,
    int TotalCount,
    int Page,
    int PageSize
);

// Provider DTOs
public record ProviderDto(
    Guid ProviderId,
    string ProviderAccountId,
    string ProviderName,
    string? ProviderCode,
    string? ProviderDescription,
    string? ProviderType,
    string? ProviderAddress,
    string? ProviderCity,
    string? ProviderState,
    string? ProviderZipCode,
    string? ProviderCountry,
    string? ProviderPhone,
    string? ProviderEmail,
    string? ProviderWebsite,
    string? ProviderContactName,
    string? ProviderAccountNumber,
    bool ProviderIsActive,
    DateTime ProviderCreatedAt,
    DateTime ProviderUpdatedAt
);

public record CreateProviderRequest(
    string ProviderAccountId,
    string ProviderName,
    string? ProviderCode,
    string? ProviderDescription,
    string? ProviderType,
    string? ProviderAddress,
    string? ProviderCity,
    string? ProviderState,
    string? ProviderZipCode,
    string? ProviderCountry,
    string? ProviderPhone,
    string? ProviderEmail,
    string? ProviderWebsite,
    string? ProviderContactName,
    string? ProviderAccountNumber
);

public record UpdateProviderRequest(
    string ProviderName,
    string? ProviderCode,
    string? ProviderDescription,
    string? ProviderType,
    string? ProviderAddress,
    string? ProviderCity,
    string? ProviderState,
    string? ProviderZipCode,
    string? ProviderCountry,
    string? ProviderPhone,
    string? ProviderEmail,
    string? ProviderWebsite,
    string? ProviderContactName,
    string? ProviderAccountNumber,
    bool ProviderIsActive
);

public record ProviderListResponse(
    List<ProviderDto> Providers,
    int TotalCount,
    int Page,
    int PageSize
);

// Utility DTOs
public record UtilityDto(
    Guid UtilityId,
    string UtilityName,
    string UtilityType,
    string? UtilityCode,
    string? UtilityDescription,
    bool UtilityIsActive,
    DateTime UtilityCreatedAt,
    DateTime UtilityUpdatedAt
);

public record CreateUtilityRequest(
    string UtilityName,
    string UtilityType,
    string? UtilityCode,
    string? UtilityDescription
);

public record UpdateUtilityRequest(
    string UtilityName,
    string UtilityType,
    string? UtilityCode,
    string? UtilityDescription,
    bool UtilityIsActive
);

public record UtilityListResponse(
    List<UtilityDto> Utilities,
    int TotalCount,
    int Page,
    int PageSize
);

// Bill Relationship DTOs
public record BillRelatedPropertyDto(
    Guid PropertyId,
    string PropertyName,
    string PropertyAddress,
    string PropertyType
);

public record BillRelatedProviderDto(
    Guid ProviderId,
    string ProviderName,
    string? ProviderType
);

public record BillRelatedUtilityDto(
    Guid UtilityId,
    string UtilityName,
    string UtilityType
);

public record BillRelationshipsDto(
    List<BillRelatedPropertyDto> Properties,
    List<BillRelatedProviderDto> Providers,
    List<BillRelatedUtilityDto> Utilities
);

// Property Relationship DTOs
public record PropertyRelatedProviderDto(
    Guid ProviderId,
    string ProviderName,
    string? ProviderType
);

public record PropertyRelatedUtilityDto(
    Guid UtilityId,
    string UtilityName,
    string UtilityType
);

public record PropertyRelatedBillDto(
    Guid BillId,
    string BillReferenceId,
    decimal BillAmount,
    string BillStatus,
    string? BillCategory
);

public record PropertyRelationshipsDto(
    List<PropertyRelatedProviderDto> Providers,
    List<PropertyRelatedUtilityDto> Utilities,
    List<PropertyRelatedBillDto> Bills
);

// Provider Relationship DTOs
public record ProviderRelatedPropertyDto(
    Guid PropertyId,
    string PropertyName,
    string? PropertyAddress,
    string? PropertyType
);

public record ProviderRelatedUtilityDto(
    Guid UtilityId,
    string UtilityName,
    string UtilityType
);

public record ProviderRelatedBillDto(
    Guid BillId,
    string BillReferenceId,
    decimal BillAmount,
    string BillStatus,
    string? BillCategory
);

public record ProviderRelationshipsDto(
    List<ProviderRelatedPropertyDto> Properties,
    List<ProviderRelatedUtilityDto> Utilities,
    List<ProviderRelatedBillDto> Bills
);

// Utility Relationship DTOs
public record UtilityRelatedPropertyDto(
    Guid PropertyId,
    string PropertyName,
    string? PropertyAddress,
    string? PropertyType
);

public record UtilityRelatedProviderDto(
    Guid ProviderId,
    string ProviderName,
    string? ProviderType
);

public record UtilityRelatedBillDto(
    Guid BillId,
    string BillReferenceId,
    decimal BillAmount,
    string BillStatus,
    string? BillCategory
);

public record UtilityRelationshipsDto(
    List<UtilityRelatedPropertyDto> Properties,
    List<UtilityRelatedProviderDto> Providers,
    List<UtilityRelatedBillDto> Bills
);
