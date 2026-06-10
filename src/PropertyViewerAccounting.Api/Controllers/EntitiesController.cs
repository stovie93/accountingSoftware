using System.Text.Json;
using PropertyViewerAccounting.Api.Services;
using PropertyViewerAccounting.Core.DTOs;
using PropertyViewerAccounting.Core.Entities;
using PropertyViewerAccounting.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PropertyViewerAccounting.Api.Controllers;

[ApiController]
[Route("api/entities")]
[Authorize]
public class EntitiesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserContext _userContext;

    public EntitiesController(AppDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<EntityDto>>> GetEntities(
        [FromQuery] Guid? entityTypeId = null,
        [FromQuery] bool includeInactive = false)
    {
        var query = _context.Entities
            .AsNoTracking()
            .Include(e => e.EntityType)
            .Where(e => e.UserId == _userContext.UserId);

        if (entityTypeId.HasValue)
        {
            query = query.Where(e => e.EntityTypeId == entityTypeId.Value);
        }

        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }

        var entities = await query
            .OrderBy(e => e.EntityType.SortOrder)
            .ThenBy(e => e.SortOrder)
            .ThenBy(e => e.Name)
            .ToListAsync();

        return Ok(entities.Select(e => MapToDto(e)).ToList());
    }

    [HttpGet("search")]
    public async Task<ActionResult<EntitySearchResponse>> SearchEntities(
        [FromQuery] string? search = null,
        [FromQuery] Guid? entityTypeId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeInactive = false)
    {
        var query = _context.Entities
            .AsNoTracking()
            .Include(e => e.EntityType)
            .Where(e => e.UserId == _userContext.UserId);

        if (entityTypeId.HasValue)
        {
            query = query.Where(e => e.EntityTypeId == entityTypeId.Value);
        }

        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(e =>
                e.Name.ToLower().Contains(searchLower) ||
                e.Code.ToLower().Contains(searchLower) ||
                (e.Description != null && e.Description.ToLower().Contains(searchLower)) ||
                e.EntityType.Name.ToLower().Contains(searchLower));
        }

        var totalCount = await query.CountAsync();

        var entities = await query
            .OrderBy(e => e.EntityType.SortOrder)
            .ThenBy(e => e.SortOrder)
            .ThenBy(e => e.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new EntitySearchResponse(
            entities.Select(e => MapToDto(e)).ToList(),
            totalCount,
            page,
            pageSize
        ));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EntityWithLinksDto>> GetEntity(Guid id)
    {
        var entity = await _context.Entities
            .AsNoTracking()
            .Include(e => e.EntityType)
            .Include(e => e.SourceLinks)
                .ThenInclude(l => l.TargetEntity)
            .Include(e => e.TargetLinks)
                .ThenInclude(l => l.SourceEntity)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == _userContext.UserId);

        if (entity == null)
        {
            return NotFound();
        }

        return Ok(MapToDtoWithLinks(entity));
    }

    [HttpPost]
    public async Task<ActionResult<EntityDto>> CreateEntity([FromBody] CreateEntityRequest request)
    {
        var entityType = await _context.EntityTypes
            .FirstOrDefaultAsync(et => et.Id == request.EntityTypeId && et.UserId == _userContext.UserId);

        if (entityType == null)
        {
            return BadRequest(new { message = "Entity type not found" });
        }

        var existingCode = await _context.Entities
            .AnyAsync(e => e.UserId == _userContext.UserId && e.Code == request.Code);

        if (existingCode)
        {
            return BadRequest(new { message = "An entity with this code already exists" });
        }

        var entity = new Entity
        {
            Id = Guid.NewGuid(),
            UserId = _userContext.UserId,
            EntityTypeId = request.EntityTypeId,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            Attributes = request.Attributes != null ? JsonSerializer.Serialize(request.Attributes) : null,
            SortOrder = request.SortOrder,
            IsActive = true,
            IsSystem = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Entities.Add(entity);
        await _context.SaveChangesAsync();

        // Reload with EntityType
        entity.EntityType = entityType;
        return CreatedAtAction(nameof(GetEntity), new { id = entity.Id }, MapToDto(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EntityDto>> UpdateEntity(Guid id, [FromBody] UpdateEntityRequest request)
    {
        var entity = await _context.Entities
            .Include(e => e.EntityType)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == _userContext.UserId);

        if (entity == null)
        {
            return NotFound();
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Attributes = request.Attributes != null ? JsonSerializer.Serialize(request.Attributes) : null;
        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapToDto(entity));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEntity(Guid id)
    {
        var entity = await _context.Entities
            .Include(e => e.ItemEntities)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == _userContext.UserId);

        if (entity == null)
        {
            return NotFound();
        }

        if (entity.ItemEntities.Any())
        {
            return BadRequest(new { message = "Cannot delete entity that has linked items. Remove item links first or deactivate the entity." });
        }

        _context.Entities.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Entity Links
    [HttpGet("{id:guid}/links")]
    public async Task<ActionResult<List<EntityLinkDto>>> GetEntityLinks(Guid id)
    {
        var entity = await _context.Entities
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == _userContext.UserId);

        if (entity == null)
        {
            return NotFound();
        }

        var links = await _context.EntityLinks
            .Include(l => l.SourceEntity)
            .Include(l => l.TargetEntity)
            .Where(l => l.UserId == _userContext.UserId &&
                       (l.SourceEntityId == id || l.TargetEntityId == id))
            .ToListAsync();

        return Ok(links.Select(l => new EntityLinkDto(
            l.Id,
            l.SourceEntityId,
            l.SourceEntity.Name,
            l.SourceEntity.Code,
            l.TargetEntityId,
            l.TargetEntity.Name,
            l.TargetEntity.Code,
            l.LinkType,
            l.Description,
            l.CreatedAt
        )).ToList());
    }

    [HttpPost("{id:guid}/links")]
    public async Task<ActionResult<EntityLinkDto>> CreateEntityLink(Guid id, [FromBody] CreateEntityLinkRequest request)
    {
        // Validate source entity
        var sourceEntity = await _context.Entities
            .FirstOrDefaultAsync(e => e.Id == request.SourceEntityId && e.UserId == _userContext.UserId);

        if (sourceEntity == null)
        {
            return BadRequest(new { message = "Source entity not found" });
        }

        // Validate target entity
        var targetEntity = await _context.Entities
            .FirstOrDefaultAsync(e => e.Id == request.TargetEntityId && e.UserId == _userContext.UserId);

        if (targetEntity == null)
        {
            return BadRequest(new { message = "Target entity not found" });
        }

        // Check for existing link
        var existingLink = await _context.EntityLinks
            .AnyAsync(l => l.UserId == _userContext.UserId &&
                          l.SourceEntityId == request.SourceEntityId &&
                          l.TargetEntityId == request.TargetEntityId &&
                          l.LinkType == request.LinkType);

        if (existingLink)
        {
            return BadRequest(new { message = "This link already exists" });
        }

        var link = new EntityLink
        {
            Id = Guid.NewGuid(),
            UserId = _userContext.UserId,
            SourceEntityId = request.SourceEntityId,
            TargetEntityId = request.TargetEntityId,
            LinkType = request.LinkType,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.EntityLinks.Add(link);
        await _context.SaveChangesAsync();

        return Ok(new EntityLinkDto(
            link.Id,
            link.SourceEntityId,
            sourceEntity.Name,
            sourceEntity.Code,
            link.TargetEntityId,
            targetEntity.Name,
            targetEntity.Code,
            link.LinkType,
            link.Description,
            link.CreatedAt
        ));
    }

    [HttpDelete("links/{linkId:guid}")]
    public async Task<IActionResult> DeleteEntityLink(Guid linkId)
    {
        var link = await _context.EntityLinks
            .FirstOrDefaultAsync(l => l.Id == linkId && l.UserId == _userContext.UserId);

        if (link == null)
        {
            return NotFound();
        }

        _context.EntityLinks.Remove(link);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static EntityDto MapToDto(Entity entity)
    {
        Dictionary<string, object>? attributes = null;
        if (!string.IsNullOrEmpty(entity.Attributes))
        {
            attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Attributes);
        }

        return new EntityDto(
            entity.Id,
            entity.EntityTypeId,
            entity.EntityType.Name,
            entity.EntityType.Code,
            entity.EntityType.Color,
            entity.Name,
            entity.Code,
            entity.Description,
            attributes,
            entity.SortOrder,
            entity.IsActive,
            entity.IsSystem,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    private static EntityWithLinksDto MapToDtoWithLinks(Entity entity)
    {
        Dictionary<string, object>? attributes = null;
        if (!string.IsNullOrEmpty(entity.Attributes))
        {
            attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Attributes);
        }

        var links = new List<EntityLinkDto>();

        // Add source links (this entity → other entities)
        foreach (var link in entity.SourceLinks)
        {
            links.Add(new EntityLinkDto(
                link.Id,
                link.SourceEntityId,
                entity.Name,
                entity.Code,
                link.TargetEntityId,
                link.TargetEntity.Name,
                link.TargetEntity.Code,
                link.LinkType,
                link.Description,
                link.CreatedAt
            ));
        }

        // Add target links (other entities → this entity)
        foreach (var link in entity.TargetLinks)
        {
            links.Add(new EntityLinkDto(
                link.Id,
                link.SourceEntityId,
                link.SourceEntity.Name,
                link.SourceEntity.Code,
                link.TargetEntityId,
                entity.Name,
                entity.Code,
                link.LinkType,
                link.Description,
                link.CreatedAt
            ));
        }

        return new EntityWithLinksDto(
            entity.Id,
            entity.EntityTypeId,
            entity.EntityType.Name,
            entity.EntityType.Code,
            entity.EntityType.Color,
            entity.Name,
            entity.Code,
            entity.Description,
            attributes,
            entity.SortOrder,
            entity.IsActive,
            entity.IsSystem,
            links,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
