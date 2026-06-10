using ConserviceAccounting.Api.Services;
using ConserviceAccounting.Core.DTOs;
using ConserviceAccounting.Core.Entities;
using ConserviceAccounting.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConserviceAccounting.Api.Controllers;

[ApiController]
[Route("api/properties")]
[Authorize]
public class PropertiesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserContext _userContext;

    public PropertiesController(AppDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<PropertyListResponse>> GetProperties(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? type = null,
        [FromQuery] string? search = null,
        [FromQuery] bool includeInactive = false)
    {
        var query = _context.Properties
            .AsNoTracking()
            .Where(p => p.UserId == _userContext.UserId);

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(p => p.PropertyType == type);
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p =>
                (p.PropertyName != null && p.PropertyName.ToLower().Contains(searchLower)) ||
                (p.PropertyAddress != null && p.PropertyAddress.ToLower().Contains(searchLower)) ||
                (p.PropertyCode != null && p.PropertyCode.ToLower().Contains(searchLower)) ||
                (p.PropertyClientId != null && p.PropertyClientId.ToLower().Contains(searchLower)));
        }

        if (!includeInactive)
        {
            query = query.Where(p => p.PropertyIsActive);
        }

        var totalCount = await query.CountAsync();

        var properties = await query
            .OrderBy(p => p.PropertyName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PropertyListResponse(
            properties.Select(MapToDto).ToList(),
            totalCount,
            page,
            pageSize
        ));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PropertyDto>> GetProperty(Guid id)
    {
        var property = await _context.Properties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PropertyId == id && p.UserId == _userContext.UserId);

        if (property == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(property));
    }

    [HttpPost]
    public async Task<ActionResult<PropertyDto>> CreateProperty([FromBody] CreatePropertyRequest request)
    {
        var existingClientId = await _context.Properties
            .AnyAsync(p => p.UserId == _userContext.UserId && p.PropertyClientId == request.PropertyClientId);

        if (existingClientId)
        {
            return BadRequest(new { message = "A property with this client ID already exists" });
        }

        var property = new Property
        {
            PropertyId = Guid.NewGuid(),
            UserId = _userContext.UserId,
            PropertyClientId = request.PropertyClientId,
            PropertyName = request.PropertyName,
            PropertyAddress = request.PropertyAddress,
            PropertyCity = request.PropertyCity,
            PropertyState = request.PropertyState,
            PropertyZipCode = request.PropertyZipCode,
            PropertyCountry = request.PropertyCountry,
            PropertyType = request.PropertyType,
            PropertyCode = request.PropertyCode,
            PropertyDescription = request.PropertyDescription,
            PropertyUnitCount = request.PropertyUnitCount,
            PropertySquareFootage = request.PropertySquareFootage,
            PropertyIsActive = true,
            PropertyCreatedAt = DateTime.UtcNow,
            PropertyUpdatedAt = DateTime.UtcNow
        };

        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProperty), new { id = property.PropertyId }, MapToDto(property));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PropertyDto>> UpdateProperty(Guid id, [FromBody] UpdatePropertyRequest request)
    {
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.PropertyId == id && p.UserId == _userContext.UserId);

        if (property == null)
        {
            return NotFound();
        }

        property.PropertyName = request.PropertyName;
        property.PropertyAddress = request.PropertyAddress;
        property.PropertyCity = request.PropertyCity;
        property.PropertyState = request.PropertyState;
        property.PropertyZipCode = request.PropertyZipCode;
        property.PropertyCountry = request.PropertyCountry;
        property.PropertyType = request.PropertyType;
        property.PropertyCode = request.PropertyCode;
        property.PropertyDescription = request.PropertyDescription;
        property.PropertyUnitCount = request.PropertyUnitCount;
        property.PropertySquareFootage = request.PropertySquareFootage;
        property.PropertyIsActive = request.PropertyIsActive;
        property.PropertyUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapToDto(property));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProperty(Guid id)
    {
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.PropertyId == id && p.UserId == _userContext.UserId);

        if (property == null)
        {
            return NotFound();
        }

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id:guid}/relationships")]
    public async Task<ActionResult<PropertyRelationshipsDto>> GetPropertyRelationships(Guid id)
    {
        var property = await _context.Properties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PropertyId == id && p.UserId == _userContext.UserId);

        if (property == null)
        {
            return NotFound();
        }

        // Get linked providers
        var providers = await _context.PropertyProviders
            .AsNoTracking()
            .Where(pp => pp.PropertyId == id)
            .Select(pp => new PropertyRelatedProviderDto(
                pp.Provider.ProviderId,
                pp.Provider.ProviderName,
                pp.Provider.ProviderType
            ))
            .ToListAsync();

        // Get linked utilities
        var utilities = await _context.PropertyUtilities
            .AsNoTracking()
            .Where(pu => pu.PropertyId == id)
            .Select(pu => new PropertyRelatedUtilityDto(
                pu.Utility.UtilityId,
                pu.Utility.UtilityName,
                pu.Utility.UtilityType
            ))
            .ToListAsync();

        // Get linked bills
        var bills = await _context.PropertyBills
            .AsNoTracking()
            .Where(pb => pb.PropertyId == id)
            .Select(pb => new PropertyRelatedBillDto(
                pb.Bill.BillId,
                pb.Bill.BillReferenceId,
                pb.Bill.BillAmount,
                pb.Bill.BillStatus,
                pb.Bill.BillCategory
            ))
            .ToListAsync();

        return Ok(new PropertyRelationshipsDto(providers, utilities, bills));
    }

    private static PropertyDto MapToDto(Property property)
    {
        return new PropertyDto(
            property.PropertyId,
            property.PropertyClientId,
            property.PropertyName,
            property.PropertyAddress,
            property.PropertyCity,
            property.PropertyState,
            property.PropertyZipCode,
            property.PropertyCountry,
            property.PropertyType,
            property.PropertyCode,
            property.PropertyDescription,
            property.PropertyUnitCount,
            property.PropertySquareFootage,
            property.PropertyIsActive,
            property.PropertyCreatedAt,
            property.PropertyUpdatedAt
        );
    }
}
