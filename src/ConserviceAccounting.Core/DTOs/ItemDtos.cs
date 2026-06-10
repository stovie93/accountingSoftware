using ConserviceAccounting.Core.Enums;

namespace ConserviceAccounting.Core.DTOs;

public record ItemDto(
    Guid Id,
    string? ExternalId,
    DateTime Date,
    decimal Amount,
    decimal? Quantity,
    string? Description,
    Dictionary<string, object>? Attributes,
    ItemSource Source,
    Guid? BatchId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateItemRequest(
    string? ExternalId,
    DateTime Date,
    decimal Amount,
    decimal? Quantity,
    string? Description,
    Dictionary<string, object>? Attributes,
    List<Guid>? EntityIds // Entities to link this item to
);

public record UpdateItemRequest(
    DateTime Date,
    decimal Amount,
    decimal? Quantity,
    string? Description,
    Dictionary<string, object>? Attributes
);

public record ItemListResponse(
    List<ItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

// Entity tag for displaying on items
public record EntityTagDto(
    Guid Id,
    string Name,
    string Code,
    string EntityTypeName,
    string EntityTypeCode,
    string? Color
);

// Item with linked entities
public record ItemWithEntitiesDto(
    Guid Id,
    List<EntityTagDto> Entities,
    string? ExternalId,
    DateTime Date,
    decimal Amount,
    decimal? Quantity,
    string? Description,
    Dictionary<string, object>? Attributes,
    ItemSource Source,
    Guid? BatchId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ItemWithEntitiesListResponse(
    List<ItemWithEntitiesDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

// Request to link/unlink entities from an item
public record UpdateItemEntitiesRequest(
    List<Guid> EntityIds
);
