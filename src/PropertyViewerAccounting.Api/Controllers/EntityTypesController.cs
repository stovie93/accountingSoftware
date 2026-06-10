using PropertyViewerAccounting.Api.Services;
using PropertyViewerAccounting.Core.DTOs;
using PropertyViewerAccounting.Core.Entities;
using PropertyViewerAccounting.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PropertyViewerAccounting.Api.Controllers;

[ApiController]
[Route("api/entity-types")]
[Authorize]
public class EntityTypesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserContext _userContext;

    public EntityTypesController(AppDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<EntityTypeDto>>> GetEntityTypes()
    {
        var entityTypes = await _context.EntityTypes
            .AsNoTracking()
            .Where(et => et.UserId == _userContext.UserId)
            .Include(et => et.Entities)
            .OrderBy(et => et.SortOrder)
            .ThenBy(et => et.Name)
            .ToListAsync();

        return Ok(entityTypes.Select(et => new EntityTypeDto(
            et.Id,
            et.Name,
            et.Code,
            et.Description,
            et.Color,
            et.Icon,
            et.SortOrder,
            et.IsSystem,
            et.Entities.Count,
            et.CreatedAt,
            et.UpdatedAt
        )).ToList());
    }

    [HttpGet("with-entities")]
    public async Task<ActionResult<List<EntityTypeWithEntitiesDto>>> GetEntityTypesWithEntities(
        [FromQuery] int maxEntitiesPerType = 50)
    {
        var entityTypes = await _context.EntityTypes
            .AsNoTracking()
            .Where(et => et.UserId == _userContext.UserId)
            .Include(et => et.Entities)
            .OrderBy(et => et.SortOrder)
            .ThenBy(et => et.Name)
            .ToListAsync();

        return Ok(entityTypes.Select(et => {
            var activeEntities = et.Entities.Where(e => e.IsActive).ToList();
            var orderedEntities = activeEntities
                .OrderBy(e => e.SortOrder)
                .ThenBy(e => e.Name)
                .Take(maxEntitiesPerType)
                .Select(e => MapToDto(e, et))
                .ToList();

            return new EntityTypeWithEntitiesDto(
                et.Id,
                et.Name,
                et.Code,
                et.Description,
                et.Color,
                et.Icon,
                et.SortOrder,
                et.IsSystem,
                orderedEntities,
                activeEntities.Count,
                et.CreatedAt,
                et.UpdatedAt
            );
        }).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EntityTypeDto>> GetEntityType(Guid id)
    {
        var entityType = await _context.EntityTypes
            .AsNoTracking()
            .Include(et => et.Entities)
            .FirstOrDefaultAsync(et => et.Id == id && et.UserId == _userContext.UserId);

        if (entityType == null)
        {
            return NotFound();
        }

        return Ok(new EntityTypeDto(
            entityType.Id,
            entityType.Name,
            entityType.Code,
            entityType.Description,
            entityType.Color,
            entityType.Icon,
            entityType.SortOrder,
            entityType.IsSystem,
            entityType.Entities.Count,
            entityType.CreatedAt,
            entityType.UpdatedAt
        ));
    }

    [HttpPost]
    public async Task<ActionResult<EntityTypeDto>> CreateEntityType([FromBody] CreateEntityTypeRequest request)
    {
        var existingCode = await _context.EntityTypes
            .AnyAsync(et => et.UserId == _userContext.UserId && et.Code == request.Code);

        if (existingCode)
        {
            return BadRequest(new { message = "An entity type with this code already exists" });
        }

        var entityType = new EntityType
        {
            Id = Guid.NewGuid(),
            UserId = _userContext.UserId,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            Color = request.Color,
            Icon = request.Icon,
            SortOrder = request.SortOrder,
            IsSystem = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EntityTypes.Add(entityType);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEntityType), new { id = entityType.Id }, new EntityTypeDto(
            entityType.Id,
            entityType.Name,
            entityType.Code,
            entityType.Description,
            entityType.Color,
            entityType.Icon,
            entityType.SortOrder,
            entityType.IsSystem,
            0,
            entityType.CreatedAt,
            entityType.UpdatedAt
        ));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EntityTypeDto>> UpdateEntityType(Guid id, [FromBody] UpdateEntityTypeRequest request)
    {
        var entityType = await _context.EntityTypes
            .Include(et => et.Entities)
            .FirstOrDefaultAsync(et => et.Id == id && et.UserId == _userContext.UserId);

        if (entityType == null)
        {
            return NotFound();
        }

        entityType.Name = request.Name;
        entityType.Description = request.Description;
        entityType.Color = request.Color;
        entityType.Icon = request.Icon;
        entityType.SortOrder = request.SortOrder;
        entityType.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new EntityTypeDto(
            entityType.Id,
            entityType.Name,
            entityType.Code,
            entityType.Description,
            entityType.Color,
            entityType.Icon,
            entityType.SortOrder,
            entityType.IsSystem,
            entityType.Entities.Count,
            entityType.CreatedAt,
            entityType.UpdatedAt
        ));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEntityType(Guid id)
    {
        var entityType = await _context.EntityTypes
            .Include(et => et.Entities)
            .FirstOrDefaultAsync(et => et.Id == id && et.UserId == _userContext.UserId);

        if (entityType == null)
        {
            return NotFound();
        }

        if (entityType.Entities.Any())
        {
            return BadRequest(new { message = "Cannot delete entity type that has entities. Delete or reassign entities first." });
        }

        _context.EntityTypes.Remove(entityType);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static EntityDto MapToDto(Entity entity, EntityType entityType)
    {
        Dictionary<string, object>? attributes = null;
        if (!string.IsNullOrEmpty(entity.Attributes))
        {
            attributes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Attributes);
        }

        return new EntityDto(
            entity.Id,
            entity.EntityTypeId,
            entityType.Name,
            entityType.Code,
            entityType.Color,
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
}
