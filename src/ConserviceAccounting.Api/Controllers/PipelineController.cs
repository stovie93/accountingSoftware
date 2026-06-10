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
[Route("api/[controller]")]
[Authorize]
public class PipelineController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserContext _userContext;

    public PipelineController(AppDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    [HttpPost("push")]
    public async Task<ActionResult<PipelinePushResponse>> Push([FromBody] PipelinePushRequest request)
    {
        var entity = await _context.Entities
            .FirstOrDefaultAsync(e => e.Code == request.EntityCode && e.UserId == _userContext.UserId);

        if (entity == null)
        {
            return BadRequest(new { message = $"Entity with code '{request.EntityCode}' not found" });
        }

        var batchId = Guid.NewGuid();

        var dataPush = new DataPush
        {
            Id = Guid.NewGuid(),
            UserId = _userContext.UserId,
            EntityId = entity.Id,
            BatchId = batchId,
            PushedAt = DateTime.UtcNow,
            RecordCount = request.Items.Count,
            Status = DataPushStatus.Processing
        };

        _context.DataPushes.Add(dataPush);

        try
        {
            foreach (var itemDto in request.Items)
            {
                // Find existing item by ExternalId only (no longer tied to a specific scope)
                var existingItem = await _context.Items
                    .Include(i => i.ItemEntities)
                    .FirstOrDefaultAsync(i =>
                        i.UserId == _userContext.UserId &&
                        i.ExternalId == itemDto.ExternalId);

                if (existingItem != null)
                {
                    existingItem.Date = DateTime.SpecifyKind(itemDto.Date, DateTimeKind.Utc);
                    existingItem.Amount = itemDto.Amount;
                    existingItem.Quantity = itemDto.Quantity;
                    existingItem.Description = itemDto.Description;
                    existingItem.Attributes = itemDto.Attributes != null
                        ? JsonSerializer.Serialize(itemDto.Attributes)
                        : null;
                    existingItem.BatchId = batchId;
                    existingItem.UpdatedAt = DateTime.UtcNow;

                    // Ensure entity link exists
                    if (!existingItem.ItemEntities.Any(ie => ie.EntityId == entity.Id))
                    {
                        _context.ItemEntities.Add(new ItemEntity
                        {
                            ItemId = existingItem.Id,
                            EntityId = entity.Id,
                            AssignedAt = DateTime.UtcNow
                        });
                    }
                }
                else
                {
                    var newItem = new Item
                    {
                        Id = Guid.NewGuid(),
                        UserId = _userContext.UserId,
                        ExternalId = itemDto.ExternalId,
                        Date = DateTime.SpecifyKind(itemDto.Date, DateTimeKind.Utc),
                        Amount = itemDto.Amount,
                        Quantity = itemDto.Quantity,
                        Description = itemDto.Description,
                        Attributes = itemDto.Attributes != null
                            ? JsonSerializer.Serialize(itemDto.Attributes)
                            : null,
                        Source = ItemSource.Pipeline,
                        BatchId = batchId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Items.Add(newItem);

                    // Link to entity
                    _context.ItemEntities.Add(new ItemEntity
                    {
                        ItemId = newItem.Id,
                        EntityId = entity.Id,
                        AssignedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();

            dataPush.Status = DataPushStatus.Completed;
            await _context.SaveChangesAsync();

            return Ok(new PipelinePushResponse(batchId, request.Items.Count, "Completed"));
        }
        catch (Exception ex)
        {
            dataPush.Status = DataPushStatus.Failed;
            dataPush.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync();

            return StatusCode(500, new { message = "Pipeline push failed", error = ex.Message });
        }
    }

    [HttpGet("status/{batchId:guid}")]
    public async Task<ActionResult<DataPush>> GetPushStatus(Guid batchId)
    {
        var dataPush = await _context.DataPushes
            .FirstOrDefaultAsync(d => d.BatchId == batchId && d.UserId == _userContext.UserId);

        if (dataPush == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            dataPush.BatchId,
            dataPush.RecordCount,
            Status = dataPush.Status.ToString(),
            dataPush.PushedAt,
            dataPush.ErrorMessage
        });
    }

    [HttpPost("entities/ensure")]
    public async Task<ActionResult<EntityDto>> EnsureEntity([FromBody] EnsureEntityRequest request)
    {
        var existingEntity = await _context.Entities
            .Include(e => e.EntityType)
            .FirstOrDefaultAsync(e => e.Code == request.Code && e.UserId == _userContext.UserId);

        if (existingEntity != null)
        {
            return Ok(new EntityDto(
                existingEntity.Id,
                existingEntity.EntityTypeId,
                existingEntity.EntityType.Name,
                existingEntity.EntityType.Code,
                existingEntity.EntityType.Color,
                existingEntity.Name,
                existingEntity.Code,
                existingEntity.Description,
                null,
                existingEntity.SortOrder,
                existingEntity.IsActive,
                existingEntity.IsSystem,
                existingEntity.CreatedAt,
                existingEntity.UpdatedAt
            ));
        }

        // Find entity type by code
        var entityType = await _context.EntityTypes
            .FirstOrDefaultAsync(et => et.Code == request.EntityTypeCode && et.UserId == _userContext.UserId);

        if (entityType == null)
        {
            return BadRequest(new { message = $"Entity type with code '{request.EntityTypeCode}' not found" });
        }

        var entity = new Entity
        {
            Id = Guid.NewGuid(),
            UserId = _userContext.UserId,
            EntityTypeId = entityType.Id,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            SortOrder = 0,
            IsActive = true,
            IsSystem = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Entities.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(EnsureEntity), new EntityDto(
            entity.Id,
            entity.EntityTypeId,
            entityType.Name,
            entityType.Code,
            entityType.Color,
            entity.Name,
            entity.Code,
            entity.Description,
            null,
            entity.SortOrder,
            entity.IsActive,
            entity.IsSystem,
            entity.CreatedAt,
            entity.UpdatedAt
        ));
    }
}

public record EnsureEntityRequest(
    string EntityTypeCode,
    string Code,
    string Name,
    string? Description
);
