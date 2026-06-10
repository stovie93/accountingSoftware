using PropertyViewerAccounting.Api.Services;
using PropertyViewerAccounting.Core.DTOs;
using PropertyViewerAccounting.Core.Entities;
using PropertyViewerAccounting.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PropertyViewerAccounting.Api.Controllers;

[ApiController]
[Route("api/utilities")]
[Authorize]
public class UtilitiesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserContext _userContext;

    public UtilitiesController(AppDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<UtilityListResponse>> GetUtilities(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? type = null,
        [FromQuery] string? search = null,
        [FromQuery] bool includeInactive = false)
    {
        var query = _context.Utilities
            .AsNoTracking()
            .Where(u => u.UserId == _userContext.UserId);

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(u => u.UtilityType == type);
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u =>
                (u.UtilityName != null && u.UtilityName.ToLower().Contains(searchLower)) ||
                (u.UtilityCode != null && u.UtilityCode.ToLower().Contains(searchLower)) ||
                (u.UtilityDescription != null && u.UtilityDescription.ToLower().Contains(searchLower)));
        }

        if (!includeInactive)
        {
            query = query.Where(u => u.UtilityIsActive);
        }

        var totalCount = await query.CountAsync();

        var utilities = await query
            .OrderBy(u => u.UtilityType)
            .ThenBy(u => u.UtilityName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new UtilityListResponse(
            utilities.Select(MapToDto).ToList(),
            totalCount,
            page,
            pageSize
        ));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UtilityDto>> GetUtility(Guid id)
    {
        var utility = await _context.Utilities
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UtilityId == id && u.UserId == _userContext.UserId);

        if (utility == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(utility));
    }

    [HttpPost]
    public async Task<ActionResult<UtilityDto>> CreateUtility([FromBody] CreateUtilityRequest request)
    {
        if (!string.IsNullOrEmpty(request.UtilityCode))
        {
            var existingCode = await _context.Utilities
                .AnyAsync(u => u.UserId == _userContext.UserId && u.UtilityCode == request.UtilityCode);

            if (existingCode)
            {
                return BadRequest(new { message = "A utility with this code already exists" });
            }
        }

        var utility = new Utility
        {
            UtilityId = Guid.NewGuid(),
            UserId = _userContext.UserId,
            UtilityName = request.UtilityName,
            UtilityType = request.UtilityType,
            UtilityCode = request.UtilityCode,
            UtilityDescription = request.UtilityDescription,
            UtilityIsActive = true,
            UtilityCreatedAt = DateTime.UtcNow,
            UtilityUpdatedAt = DateTime.UtcNow
        };

        _context.Utilities.Add(utility);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUtility), new { id = utility.UtilityId }, MapToDto(utility));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UtilityDto>> UpdateUtility(Guid id, [FromBody] UpdateUtilityRequest request)
    {
        var utility = await _context.Utilities
            .FirstOrDefaultAsync(u => u.UtilityId == id && u.UserId == _userContext.UserId);

        if (utility == null)
        {
            return NotFound();
        }

        utility.UtilityName = request.UtilityName;
        utility.UtilityType = request.UtilityType;
        utility.UtilityCode = request.UtilityCode;
        utility.UtilityDescription = request.UtilityDescription;
        utility.UtilityIsActive = request.UtilityIsActive;
        utility.UtilityUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapToDto(utility));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUtility(Guid id)
    {
        var utility = await _context.Utilities
            .FirstOrDefaultAsync(u => u.UtilityId == id && u.UserId == _userContext.UserId);

        if (utility == null)
        {
            return NotFound();
        }

        _context.Utilities.Remove(utility);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id:guid}/relationships")]
    public async Task<ActionResult<UtilityRelationshipsDto>> GetUtilityRelationships(Guid id)
    {
        var utility = await _context.Utilities
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UtilityId == id && u.UserId == _userContext.UserId);

        if (utility == null)
        {
            return NotFound();
        }

        // Get linked properties
        var properties = await _context.PropertyUtilities
            .AsNoTracking()
            .Where(pu => pu.UtilityId == id)
            .Select(pu => new UtilityRelatedPropertyDto(
                pu.Property.PropertyId,
                pu.Property.PropertyName,
                pu.Property.PropertyAddress,
                pu.Property.PropertyType
            ))
            .ToListAsync();

        // Get linked providers
        var providers = await _context.ProviderUtilities
            .AsNoTracking()
            .Where(pu => pu.UtilityId == id)
            .Select(pu => new UtilityRelatedProviderDto(
                pu.Provider.ProviderId,
                pu.Provider.ProviderName,
                pu.Provider.ProviderType
            ))
            .ToListAsync();

        // Get linked bills
        var bills = await _context.BillUtilities
            .AsNoTracking()
            .Where(bu => bu.UtilityId == id)
            .Select(bu => new UtilityRelatedBillDto(
                bu.Bill.BillId,
                bu.Bill.BillReferenceId,
                bu.Bill.BillAmount,
                bu.Bill.BillStatus,
                bu.Bill.BillCategory
            ))
            .ToListAsync();

        return Ok(new UtilityRelationshipsDto(properties, providers, bills));
    }

    private static UtilityDto MapToDto(Utility utility)
    {
        return new UtilityDto(
            utility.UtilityId,
            utility.UtilityName,
            utility.UtilityType,
            utility.UtilityCode,
            utility.UtilityDescription,
            utility.UtilityIsActive,
            utility.UtilityCreatedAt,
            utility.UtilityUpdatedAt
        );
    }
}
