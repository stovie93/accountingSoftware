using ConserviceAccounting.Core.Enums;

namespace ConserviceAccounting.Core.DTOs;

// Report DTOs
public record ReportDto(
    Guid Id,
    string Name,
    ReportType Type,
    string Configuration,
    bool IsShared,
    Guid CreatedById,
    string? CreatedByName,
    DateTime CreatedAt
);

public record CreateReportRequest(
    string Name,
    ReportType Type,
    string Configuration,
    bool IsShared = false
);

public record UpdateReportRequest(
    string Name,
    ReportType Type,
    string Configuration,
    bool IsShared
);

public record ReportListResponse(
    List<ReportDto> Reports,
    int TotalCount,
    int Page,
    int PageSize
);

// Export request for generating file
public record ExportReportRequest(
    string Format = "csv"  // csv or xlsx
);
