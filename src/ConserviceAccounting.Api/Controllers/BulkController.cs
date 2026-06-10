using System.Text.Json;
using ConserviceAccounting.Api.Services;
using ConserviceAccounting.Core.DTOs;
using ConserviceAccounting.Core.Entities;
using ConserviceAccounting.Core.Enums;
using ConserviceAccounting.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConserviceAccounting.Api.Controllers;

[ApiController]
[Route("api/bulk")]
[Authorize]
public class BulkController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserContext _userContext;
    private const int BatchSize = 100;

    public BulkController(AppDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    /// <summary>
    /// Bulk import items with batch processing for scalability.
    /// Processes items in batches of 100 to avoid memory issues with large imports.
    /// </summary>
    [HttpPost("items")]
    public async Task<ActionResult<BulkImportResponse>> BulkImportItems([FromBody] BulkImportItemsRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest(new { message = "No items provided" });
        }

        var batchId = Guid.NewGuid();
        var results = new BulkImportResponse
        {
            BatchId = batchId,
            TotalRequested = request.Items.Count,
            Succeeded = 0,
            Failed = 0,
            Errors = new List<BulkImportError>()
        };

        // Process in batches
        var batches = request.Items
            .Select((item, index) => new { Item = item, Index = index })
            .GroupBy(x => x.Index / BatchSize)
            .Select(g => g.Select(x => new { x.Item, x.Index }).ToList())
            .ToList();

        foreach (var batch in batches)
        {
            var itemsToAdd = new List<Item>();
            var itemEntitiesToAdd = new List<ItemEntity>();

            foreach (var entry in batch)
            {
                try
                {
                    var item = new Item
                    {
                        Id = Guid.NewGuid(),
                        UserId = _userContext.UserId,
                        ExternalId = entry.Item.ExternalId,
                        Date = entry.Item.Date,
                        Amount = entry.Item.Amount,
                        Quantity = entry.Item.Quantity,
                        Description = entry.Item.Description,
                        Attributes = entry.Item.Attributes != null
                            ? JsonSerializer.Serialize(entry.Item.Attributes)
                            : null,
                        Source = ItemSource.Import,
                        BatchId = batchId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    itemsToAdd.Add(item);

                    // Queue entity links
                    if (entry.Item.EntityIds != null)
                    {
                        foreach (var entityId in entry.Item.EntityIds)
                        {
                            itemEntitiesToAdd.Add(new ItemEntity
                            {
                                ItemId = item.Id,
                                EntityId = entityId,
                                AssignedAt = DateTime.UtcNow
                            });
                        }
                    }

                    results.Succeeded++;
                }
                catch (Exception ex)
                {
                    results.Failed++;
                    results.Errors.Add(new BulkImportError
                    {
                        Index = entry.Index,
                        ExternalId = entry.Item.ExternalId,
                        Message = ex.Message
                    });
                }
            }

            // Bulk insert the batch
            if (itemsToAdd.Count > 0)
            {
                await _context.Items.AddRangeAsync(itemsToAdd);

                if (itemEntitiesToAdd.Count > 0)
                {
                    await _context.ItemEntities.AddRangeAsync(itemEntitiesToAdd);
                }

                await _context.SaveChangesAsync();
            }
        }

        return Ok(results);
    }

    /// <summary>
    /// Get items by batch ID for tracking bulk import results.
    /// </summary>
    [HttpGet("items/batch/{batchId:guid}")]
    public async Task<ActionResult<ItemListResponse>> GetBatchItems(
        Guid batchId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.Items
            .AsNoTracking()
            .Where(i => i.UserId == _userContext.UserId && i.BatchId == batchId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new ItemListResponse(
            items.Select(MapToDto).ToList(),
            totalCount,
            page,
            pageSize
        ));
    }

    /// <summary>
    /// Delete all items from a specific batch (for rollback scenarios).
    /// </summary>
    [HttpDelete("items/batch/{batchId:guid}")]
    public async Task<ActionResult<BulkDeleteResponse>> DeleteBatch(Guid batchId)
    {
        var items = await _context.Items
            .Where(i => i.UserId == _userContext.UserId && i.BatchId == batchId)
            .ToListAsync();

        if (items.Count == 0)
        {
            return NotFound(new { message = "No items found for this batch" });
        }

        _context.Items.RemoveRange(items);
        await _context.SaveChangesAsync();

        return Ok(new BulkDeleteResponse
        {
            BatchId = batchId,
            DeletedCount = items.Count
        });
    }

    private static ItemDto MapToDto(Item item)
    {
        Dictionary<string, object>? attributes = null;
        if (!string.IsNullOrEmpty(item.Attributes))
        {
            attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(item.Attributes);
        }

        return new ItemDto(
            item.Id,
            item.ExternalId,
            item.Date,
            item.Amount,
            item.Quantity,
            item.Description,
            attributes,
            item.Source,
            item.BatchId,
            item.CreatedAt,
            item.UpdatedAt
        );
    }
}

// DTOs for bulk operations
public record BulkImportItemsRequest(List<BulkItemInput> Items);

public record BulkItemInput(
    string? ExternalId,
    DateTime Date,
    decimal Amount,
    decimal? Quantity,
    string? Description,
    Dictionary<string, object>? Attributes,
    List<Guid>? EntityIds
);

public record BulkImportResponse
{
    public Guid BatchId { get; set; }
    public int TotalRequested { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public List<BulkImportError> Errors { get; set; } = new();
}

public record BulkImportError
{
    public int Index { get; set; }
    public string? ExternalId { get; set; }
    public string Message { get; set; } = "";
}

public record BulkDeleteResponse
{
    public Guid BatchId { get; set; }
    public int DeletedCount { get; set; }
}
