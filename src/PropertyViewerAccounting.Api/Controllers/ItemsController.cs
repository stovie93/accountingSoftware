using System.Text.Json;
using PropertyViewerAccounting.Api.Services;
using PropertyViewerAccounting.Core.DTOs;
using PropertyViewerAccounting.Core.Entities;
using PropertyViewerAccounting.Core.Enums;
using PropertyViewerAccounting.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PropertyViewerAccounting.Api.Controllers;

[ApiController]
[Route("api/items")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserContext _userContext;

    public ItemsController(AppDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    /// <summary>
    /// Get items with multi-entity filtering. Items can be linked to multiple entities.
    /// Use entityIds to filter items that have ALL specified entities (AND logic) or ANY (OR logic).
    /// Use includeEntities=false for faster queries when entity details are not needed.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ItemWithEntitiesListResponse>> GetItems(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] Guid[]? entityIds = null,
        [FromQuery] string? entityMatch = "any", // "any" = OR, "all" = AND
        [FromQuery] string? search = null,
        [FromQuery] bool includeEntities = true)
    {
        var baseQuery = _context.Items
            .AsNoTracking()
            .Where(i => i.UserId == _userContext.UserId);

        // Only include entity relationships when needed (reduces query overhead)
        IQueryable<Item> query = includeEntities
            ? baseQuery
                .Include(i => i.ItemEntities)
                    .ThenInclude(ie => ie.Entity)
                        .ThenInclude(e => e.EntityType)
            : baseQuery;

        // Handle entity filtering via ItemEntities junction table
        var filterEntityIds = entityIds?.Length > 0 ? entityIds.ToList() : (entityId.HasValue ? new List<Guid> { entityId.Value } : null);

        if (filterEntityIds != null && filterEntityIds.Count > 0)
        {
            if (entityMatch == "all")
            {
                // AND logic: item must have ALL specified entities
                foreach (var eid in filterEntityIds)
                {
                    query = query.Where(i => i.ItemEntities.Any(ie => ie.EntityId == eid));
                }
            }
            else
            {
                // OR logic (default): item must have ANY of the specified entities
                query = query.Where(i => i.ItemEntities.Any(ie => filterEntityIds.Contains(ie.EntityId)));
            }
        }

        if (startDate.HasValue)
        {
            query = query.Where(i => i.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(i => i.Date <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i =>
                (i.Description != null && i.Description.ToLower().Contains(search.ToLower())) ||
                (i.ExternalId != null && i.ExternalId.ToLower().Contains(search.ToLower())));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(i => i.Date)
            .ThenByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new ItemWithEntitiesListResponse(
            items.Select(i => MapToDtoWithEntities(i)).ToList(),
            totalCount,
            page,
            pageSize
        ));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ItemWithEntitiesDto>> GetItem(Guid id, [FromQuery] bool includeEntities = true)
    {
        var baseQuery = _context.Items
            .AsNoTracking()
            .Where(i => i.Id == id && i.UserId == _userContext.UserId);

        var item = includeEntities
            ? await baseQuery
                .Include(i => i.ItemEntities)
                    .ThenInclude(ie => ie.Entity)
                        .ThenInclude(e => e.EntityType)
                .FirstOrDefaultAsync()
            : await baseQuery.FirstOrDefaultAsync();

        if (item == null)
        {
            return NotFound();
        }

        return Ok(MapToDtoWithEntities(item));
    }

    [HttpPost]
    public async Task<ActionResult<ItemWithEntitiesDto>> CreateItem([FromBody] CreateItemRequest request)
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            UserId = _userContext.UserId,
            ExternalId = request.ExternalId,
            Date = request.Date,
            Amount = request.Amount,
            Quantity = request.Quantity,
            Description = request.Description,
            Attributes = request.Attributes != null ? JsonSerializer.Serialize(request.Attributes) : null,
            Source = ItemSource.Manual,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Items.Add(item);

        // Link to entities if provided
        if (request.EntityIds != null && request.EntityIds.Any())
        {
            foreach (var entityId in request.EntityIds)
            {
                var entityExists = await _context.Entities
                    .AnyAsync(e => e.Id == entityId && e.UserId == _userContext.UserId);

                if (entityExists)
                {
                    _context.ItemEntities.Add(new ItemEntity
                    {
                        ItemId = item.Id,
                        EntityId = entityId,
                        AssignedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await _context.SaveChangesAsync();

        // Reload with entities
        var savedItem = await _context.Items
            .Include(i => i.ItemEntities)
                .ThenInclude(ie => ie.Entity)
                    .ThenInclude(e => e.EntityType)
            .FirstAsync(i => i.Id == item.Id);

        return CreatedAtAction(nameof(GetItem), new { id = item.Id }, MapToDtoWithEntities(savedItem));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ItemDto>> UpdateItem(Guid id, [FromBody] UpdateItemRequest request)
    {
        var item = await _context.Items
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == _userContext.UserId);

        if (item == null)
        {
            return NotFound();
        }

        item.Date = request.Date;
        item.Amount = request.Amount;
        item.Quantity = request.Quantity;
        item.Description = request.Description;
        item.Attributes = request.Attributes != null ? JsonSerializer.Serialize(request.Attributes) : null;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapToDto(item));
    }

    [HttpPut("{id:guid}/entities")]
    public async Task<ActionResult<ItemWithEntitiesDto>> UpdateItemEntities(Guid id, [FromBody] UpdateItemEntitiesRequest request)
    {
        var item = await _context.Items
            .Include(i => i.ItemEntities)
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == _userContext.UserId);

        if (item == null)
        {
            return NotFound();
        }

        // Remove existing entity links
        _context.ItemEntities.RemoveRange(item.ItemEntities);

        // Add new entity links
        foreach (var entityId in request.EntityIds)
        {
            var entityExists = await _context.Entities
                .AnyAsync(e => e.Id == entityId && e.UserId == _userContext.UserId);

            if (entityExists)
            {
                _context.ItemEntities.Add(new ItemEntity
                {
                    ItemId = item.Id,
                    EntityId = entityId,
                    AssignedAt = DateTime.UtcNow
                });
            }
        }

        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Reload with entities
        var savedItem = await _context.Items
            .Include(i => i.ItemEntities)
                .ThenInclude(ie => ie.Entity)
                    .ThenInclude(e => e.EntityType)
            .FirstAsync(i => i.Id == item.Id);

        return Ok(MapToDtoWithEntities(savedItem));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        var item = await _context.Items
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == _userContext.UserId);

        if (item == null)
        {
            return NotFound();
        }

        _context.Items.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
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

    private static ItemWithEntitiesDto MapToDtoWithEntities(Item item)
    {
        Dictionary<string, object>? attributes = null;
        if (!string.IsNullOrEmpty(item.Attributes))
        {
            attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(item.Attributes);
        }

        var entities = item.ItemEntities?
            .Where(ie => ie.Entity != null)
            .Select(ie => new EntityTagDto(
                ie.Entity.Id,
                ie.Entity.Name,
                ie.Entity.Code,
                ie.Entity.EntityType?.Name ?? "",
                ie.Entity.EntityType?.Code ?? "",
                ie.Entity.EntityType?.Color
            ))
            .ToList() ?? new List<EntityTagDto>();

        return new ItemWithEntitiesDto(
            item.Id,
            entities,
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
