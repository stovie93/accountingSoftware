namespace PropertyViewerAccounting.Core.DTOs;

// EntityType DTOs
public record EntityTypeDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    string? Color,
    string? Icon,
    int SortOrder,
    bool IsSystem,
    int EntityCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record EntityTypeWithEntitiesDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    string? Color,
    string? Icon,
    int SortOrder,
    bool IsSystem,
    List<EntityDto> Entities,
    int TotalEntityCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateEntityTypeRequest(
    string Name,
    string Code,
    string? Description,
    string? Color,
    string? Icon,
    int SortOrder = 0
);

public record UpdateEntityTypeRequest(
    string Name,
    string? Description,
    string? Color,
    string? Icon,
    int SortOrder
);

// Entity DTOs
public record EntityDto(
    Guid Id,
    Guid EntityTypeId,
    string EntityTypeName,
    string EntityTypeCode,
    string? EntityTypeColor,
    string Name,
    string Code,
    string? Description,
    Dictionary<string, object>? Attributes,
    int SortOrder,
    bool IsActive,
    bool IsSystem,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record EntityWithLinksDto(
    Guid Id,
    Guid EntityTypeId,
    string EntityTypeName,
    string EntityTypeCode,
    string? EntityTypeColor,
    string Name,
    string Code,
    string? Description,
    Dictionary<string, object>? Attributes,
    int SortOrder,
    bool IsActive,
    bool IsSystem,
    List<EntityLinkDto> Links,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateEntityRequest(
    Guid EntityTypeId,
    string Name,
    string Code,
    string? Description,
    Dictionary<string, object>? Attributes,
    int SortOrder = 0
);

public record UpdateEntityRequest(
    string Name,
    string? Description,
    Dictionary<string, object>? Attributes,
    bool IsActive,
    int SortOrder
);

// EntityLink DTOs
public record EntityLinkDto(
    Guid Id,
    Guid SourceEntityId,
    string SourceEntityName,
    string SourceEntityCode,
    Guid TargetEntityId,
    string TargetEntityName,
    string TargetEntityCode,
    string LinkType,
    string? Description,
    DateTime CreatedAt
);

public record CreateEntityLinkRequest(
    Guid SourceEntityId,
    Guid TargetEntityId,
    string LinkType,
    string? Description
);

public record EntitySearchResponse(
    List<EntityDto> Entities,
    int TotalCount,
    int Page,
    int PageSize
);
