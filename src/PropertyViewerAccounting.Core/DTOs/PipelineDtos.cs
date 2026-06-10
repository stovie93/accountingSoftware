namespace PropertyViewerAccounting.Core.DTOs;

public record PipelinePushRequest(
    string EntityCode,
    List<PipelineItemDto> Items
);

public record PipelineItemDto(
    string ExternalId,
    DateTime Date,
    decimal Amount,
    decimal? Quantity,
    string? Description,
    Dictionary<string, object>? Attributes
);

public record PipelinePushResponse(
    Guid BatchId,
    int RecordCount,
    string Status
);

