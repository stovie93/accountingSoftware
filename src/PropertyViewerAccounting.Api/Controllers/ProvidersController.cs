using PropertyViewerAccounting.Api.Services;
using PropertyViewerAccounting.Core.DTOs;
using PropertyViewerAccounting.Core.Entities;
using PropertyViewerAccounting.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PropertyViewerAccounting.Api.Controllers;

[ApiController]
[Route("api/providers")]
[Authorize]
public class ProvidersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserContext _userContext;

    public ProvidersController(AppDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<ProviderListResponse>> GetProviders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? type = null,
        [FromQuery] string? search = null,
        [FromQuery] bool includeInactive = false)
    {
        var query = _context.Providers
            .AsNoTracking()
            .Where(p => p.UserId == _userContext.UserId);

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(p => p.ProviderType == type);
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p =>
                (p.ProviderName != null && p.ProviderName.ToLower().Contains(searchLower)) ||
                (p.ProviderCode != null && p.ProviderCode.ToLower().Contains(searchLower)) ||
                (p.ProviderAccountNumber != null && p.ProviderAccountNumber.ToLower().Contains(searchLower)));
        }

        if (!includeInactive)
        {
            query = query.Where(p => p.ProviderIsActive);
        }

        var totalCount = await query.CountAsync();

        var providers = await query
            .OrderBy(p => p.ProviderName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new ProviderListResponse(
            providers.Select(MapToDto).ToList(),
            totalCount,
            page,
            pageSize
        ));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProviderDto>> GetProvider(Guid id)
    {
        var provider = await _context.Providers
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProviderId == id && p.UserId == _userContext.UserId);

        if (provider == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(provider));
    }

    [HttpPost]
    public async Task<ActionResult<ProviderDto>> CreateProvider([FromBody] CreateProviderRequest request)
    {
        var existingAccountId = await _context.Providers
            .AnyAsync(p => p.UserId == _userContext.UserId && p.ProviderAccountId == request.ProviderAccountId);

        if (existingAccountId)
        {
            return BadRequest(new { message = "A provider with this account ID already exists" });
        }

        var provider = new Provider
        {
            ProviderId = Guid.NewGuid(),
            UserId = _userContext.UserId,
            ProviderAccountId = request.ProviderAccountId,
            ProviderName = request.ProviderName,
            ProviderCode = request.ProviderCode,
            ProviderDescription = request.ProviderDescription,
            ProviderType = request.ProviderType,
            ProviderAddress = request.ProviderAddress,
            ProviderCity = request.ProviderCity,
            ProviderState = request.ProviderState,
            ProviderZipCode = request.ProviderZipCode,
            ProviderCountry = request.ProviderCountry,
            ProviderPhone = request.ProviderPhone,
            ProviderEmail = request.ProviderEmail,
            ProviderWebsite = request.ProviderWebsite,
            ProviderContactName = request.ProviderContactName,
            ProviderAccountNumber = request.ProviderAccountNumber,
            ProviderIsActive = true,
            ProviderCreatedAt = DateTime.UtcNow,
            ProviderUpdatedAt = DateTime.UtcNow
        };

        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProvider), new { id = provider.ProviderId }, MapToDto(provider));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProviderDto>> UpdateProvider(Guid id, [FromBody] UpdateProviderRequest request)
    {
        var provider = await _context.Providers
            .FirstOrDefaultAsync(p => p.ProviderId == id && p.UserId == _userContext.UserId);

        if (provider == null)
        {
            return NotFound();
        }

        provider.ProviderName = request.ProviderName;
        provider.ProviderCode = request.ProviderCode;
        provider.ProviderDescription = request.ProviderDescription;
        provider.ProviderType = request.ProviderType;
        provider.ProviderAddress = request.ProviderAddress;
        provider.ProviderCity = request.ProviderCity;
        provider.ProviderState = request.ProviderState;
        provider.ProviderZipCode = request.ProviderZipCode;
        provider.ProviderCountry = request.ProviderCountry;
        provider.ProviderPhone = request.ProviderPhone;
        provider.ProviderEmail = request.ProviderEmail;
        provider.ProviderWebsite = request.ProviderWebsite;
        provider.ProviderContactName = request.ProviderContactName;
        provider.ProviderAccountNumber = request.ProviderAccountNumber;
        provider.ProviderIsActive = request.ProviderIsActive;
        provider.ProviderUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapToDto(provider));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProvider(Guid id)
    {
        var provider = await _context.Providers
            .FirstOrDefaultAsync(p => p.ProviderId == id && p.UserId == _userContext.UserId);

        if (provider == null)
        {
            return NotFound();
        }

        _context.Providers.Remove(provider);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id:guid}/relationships")]
    public async Task<ActionResult<ProviderRelationshipsDto>> GetProviderRelationships(Guid id)
    {
        var provider = await _context.Providers
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProviderId == id && p.UserId == _userContext.UserId);

        if (provider == null)
        {
            return NotFound();
        }

        // Get linked properties
        var properties = await _context.PropertyProviders
            .AsNoTracking()
            .Where(pp => pp.ProviderId == id)
            .Select(pp => new ProviderRelatedPropertyDto(
                pp.Property.PropertyId,
                pp.Property.PropertyName,
                pp.Property.PropertyAddress,
                pp.Property.PropertyType
            ))
            .ToListAsync();

        // Get linked utilities
        var utilities = await _context.ProviderUtilities
            .AsNoTracking()
            .Where(pu => pu.ProviderId == id)
            .Select(pu => new ProviderRelatedUtilityDto(
                pu.Utility.UtilityId,
                pu.Utility.UtilityName,
                pu.Utility.UtilityType
            ))
            .ToListAsync();

        // Get linked bills
        var bills = await _context.BillProviders
            .AsNoTracking()
            .Where(bp => bp.ProviderId == id)
            .Select(bp => new ProviderRelatedBillDto(
                bp.Bill.BillId,
                bp.Bill.BillReferenceId,
                bp.Bill.BillAmount,
                bp.Bill.BillStatus,
                bp.Bill.BillCategory
            ))
            .ToListAsync();

        return Ok(new ProviderRelationshipsDto(properties, utilities, bills));
    }

    private static ProviderDto MapToDto(Provider provider)
    {
        return new ProviderDto(
            provider.ProviderId,
            provider.ProviderAccountId,
            provider.ProviderName,
            provider.ProviderCode,
            provider.ProviderDescription,
            provider.ProviderType,
            provider.ProviderAddress,
            provider.ProviderCity,
            provider.ProviderState,
            provider.ProviderZipCode,
            provider.ProviderCountry,
            provider.ProviderPhone,
            provider.ProviderEmail,
            provider.ProviderWebsite,
            provider.ProviderContactName,
            provider.ProviderAccountNumber,
            provider.ProviderIsActive,
            provider.ProviderCreatedAt,
            provider.ProviderUpdatedAt
        );
    }
}
