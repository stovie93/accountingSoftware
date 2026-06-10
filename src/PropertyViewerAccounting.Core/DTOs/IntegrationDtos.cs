using PropertyViewerAccounting.Core.Enums;

namespace PropertyViewerAccounting.Core.DTOs;

// Accounting Software DTOs
public record AccountingSoftwareDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    string ConnectionType,
    string? LogoUrl,
    bool IsActive
);

public record AccountingSoftwareListResponse(
    List<AccountingSoftwareDto> Software,
    int TotalCount
);

// Client Accounting Connection DTOs
public record ClientAccountingConnectionDto(
    Guid Id,
    Guid AccountingSoftwareId,
    string AccountingSoftwareName,
    string Name,
    string ConnectionType,
    bool IsActive,
    DateTime? LastTestedAt,
    string? LastTestStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ClientAccountingConnectionDetailDto(
    Guid Id,
    Guid AccountingSoftwareId,
    string AccountingSoftwareName,
    string Name,
    // Database fields (masked for security)
    string? DatabaseServer,
    string? DatabaseName,
    string? DatabaseUsername,
    bool HasDatabasePassword,
    // API fields (masked)
    string? ApiEndpoint,
    bool HasApiKey,
    bool HasApiSecret,
    // SFTP fields (masked)
    string? SftpHost,
    int? SftpPort,
    string? SftpUsername,
    bool HasSftpPassword,
    string? SftpPath,
    // Status
    bool IsActive,
    DateTime? LastTestedAt,
    string? LastTestStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateConnectionRequest(
    Guid AccountingSoftwareId,
    string Name,
    // Database connection
    string? ConnectionString,
    string? DatabaseName,
    string? DatabaseServer,
    string? DatabaseUsername,
    string? DatabasePassword,
    // API connection
    string? ApiEndpoint,
    string? ApiKey,
    string? ApiSecret,
    // SFTP connection
    string? SftpHost,
    int? SftpPort,
    string? SftpUsername,
    string? SftpPassword,
    string? SftpPath,
    bool IsActive = true
);

public record UpdateConnectionRequest(
    string Name,
    // Database connection
    string? ConnectionString,
    string? DatabaseName,
    string? DatabaseServer,
    string? DatabaseUsername,
    string? DatabasePassword,
    // API connection
    string? ApiEndpoint,
    string? ApiKey,
    string? ApiSecret,
    // SFTP connection
    string? SftpHost,
    int? SftpPort,
    string? SftpUsername,
    string? SftpPassword,
    string? SftpPath,
    bool IsActive
);

public record ConnectionListResponse(
    List<ClientAccountingConnectionDto> Connections,
    int TotalCount,
    int Page,
    int PageSize
);

public record TestConnectionResult(
    bool Success,
    string Message,
    DateTime TestedAt
);

// Report Schedule DTOs
public record ReportScheduleDto(
    Guid Id,
    Guid ReportDefinitionId,
    string ReportName,
    Guid ConnectionId,
    string ConnectionName,
    string AccountingSoftwareName,
    string Name,
    string? Description,
    ScheduleFrequency Frequency,
    string? CronExpression,
    TimeOnly TimeOfDay,
    int? DayOfWeek,
    int? DayOfMonth,
    DateTime StartDate,
    DateTime? EndDate,
    string ExportFormat,
    string? DestinationTable,
    string? DestinationPath,
    bool IsActive,
    DateTime? LastRunAt,
    string? LastRunStatus,
    DateTime? NextRunAt,
    DateTime CreatedAt
);

public record CreateScheduleRequest(
    Guid ReportDefinitionId,
    Guid ConnectionId,
    string Name,
    string? Description,
    ScheduleFrequency Frequency,
    string? CronExpression,
    TimeOnly? TimeOfDay,
    int? DayOfWeek,
    int? DayOfMonth,
    DateTime? StartDate,
    DateTime? EndDate,
    string ExportFormat = "csv",
    string? DestinationTable = null,
    string? DestinationPath = null,
    bool IsActive = true
);

public record UpdateScheduleRequest(
    string Name,
    string? Description,
    ScheduleFrequency Frequency,
    string? CronExpression,
    TimeOnly? TimeOfDay,
    int? DayOfWeek,
    int? DayOfMonth,
    DateTime? StartDate,
    DateTime? EndDate,
    string ExportFormat,
    string? DestinationTable,
    string? DestinationPath,
    bool IsActive
);

public record ScheduleListResponse(
    List<ReportScheduleDto> Schedules,
    int TotalCount,
    int Page,
    int PageSize
);

// Report Schedule Run DTOs
public record ReportScheduleRunDto(
    Guid Id,
    Guid ScheduleId,
    ScheduleRunStatus Status,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    int RecordsProcessed,
    int RecordsFailed,
    string? ErrorMessage,
    string? OutputFilePath,
    long? FileSizeBytes
);

public record ScheduleRunListResponse(
    List<ReportScheduleRunDto> Runs,
    int TotalCount,
    int Page,
    int PageSize
);

// Manual run request
public record TriggerScheduleRunRequest(
    bool Force = false
);
