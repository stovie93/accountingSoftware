using PropertyViewerAccounting.Api.Services;
using PropertyViewerAccounting.Core.DTOs;
using PropertyViewerAccounting.Core.Entities;
using PropertyViewerAccounting.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PropertyViewerAccounting.Api.Controllers;

[ApiController]
[Route("api/bills")]
[Authorize]
public class BillsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserContext _userContext;

    public BillsController(AppDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<BillListResponse>> GetBills(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        [FromQuery] bool includeInactive = false)
    {
        var query = _context.Bills
            .AsNoTracking()
            .Where(b => b.UserId == _userContext.UserId);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(b => b.BillStatus == status);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(b => b.BillDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(b => b.BillDate <= toDate.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(b =>
                (b.BillReferenceId != null && b.BillReferenceId.ToLower().Contains(searchLower)) ||
                (b.BillDescription != null && b.BillDescription.ToLower().Contains(searchLower)) ||
                (b.BillExternalId != null && b.BillExternalId.ToLower().Contains(searchLower)));
        }

        if (!includeInactive)
        {
            query = query.Where(b => b.BillIsActive);
        }

        var totalCount = await query.CountAsync();

        var bills = await query
            .Include(b => b.BillUtilities)
                .ThenInclude(bu => bu.Utility)
            .Include(b => b.BillProviders)
                .ThenInclude(bp => bp.Provider)
            .OrderByDescending(b => b.BillDate)
            .ThenBy(b => b.BillReferenceId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new BillListResponse(
            bills.Select(MapToDto).ToList(),
            totalCount,
            page,
            pageSize
        ));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BillDto>> GetBill(Guid id)
    {
        var bill = await _context.Bills
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BillId == id && b.UserId == _userContext.UserId);

        if (bill == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(bill));
    }

    [HttpPost]
    public async Task<ActionResult<BillDto>> CreateBill([FromBody] CreateBillRequest request)
    {
        var existingRef = await _context.Bills
            .AnyAsync(b => b.UserId == _userContext.UserId && b.BillReferenceId == request.BillReferenceId);

        if (existingRef)
        {
            return BadRequest(new { message = "A bill with this reference ID already exists" });
        }

        var bill = new Bill
        {
            BillId = Guid.NewGuid(),
            UserId = _userContext.UserId,
            BillReferenceId = request.BillReferenceId,
            BillAmount = request.BillAmount,
            BillStatus = request.BillStatus,
            BillDescription = request.BillDescription,
            BillDate = request.BillDate,
            BillDueDate = request.BillDueDate,
            BillPeriodStart = request.BillPeriodStart,
            BillPeriodEnd = request.BillPeriodEnd,
            BillQuantity = request.BillQuantity,
            BillUnit = request.BillUnit,
            BillRate = request.BillRate,
            BillTax = request.BillTax,
            BillTotalAmount = request.BillTotalAmount,
            BillCurrency = request.BillCurrency,
            BillNotes = request.BillNotes,
            BillExternalId = request.BillExternalId,
            BillIsActive = true,
            BillCreatedAt = DateTime.UtcNow,
            BillUpdatedAt = DateTime.UtcNow
        };

        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBill), new { id = bill.BillId }, MapToDto(bill));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BillDto>> UpdateBill(Guid id, [FromBody] UpdateBillRequest request)
    {
        var bill = await _context.Bills
            .FirstOrDefaultAsync(b => b.BillId == id && b.UserId == _userContext.UserId);

        if (bill == null)
        {
            return NotFound();
        }

        bill.BillAmount = request.BillAmount;
        bill.BillStatus = request.BillStatus;
        bill.BillDescription = request.BillDescription;
        bill.BillDate = request.BillDate;
        bill.BillDueDate = request.BillDueDate;
        bill.BillPaidDate = request.BillPaidDate;
        bill.BillPeriodStart = request.BillPeriodStart;
        bill.BillPeriodEnd = request.BillPeriodEnd;
        bill.BillQuantity = request.BillQuantity;
        bill.BillUnit = request.BillUnit;
        bill.BillRate = request.BillRate;
        bill.BillTax = request.BillTax;
        bill.BillTotalAmount = request.BillTotalAmount;
        bill.BillCurrency = request.BillCurrency;
        bill.BillNotes = request.BillNotes;
        bill.BillExternalId = request.BillExternalId;
        bill.BillIsActive = request.BillIsActive;
        bill.BillUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapToDto(bill));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBill(Guid id)
    {
        var bill = await _context.Bills
            .FirstOrDefaultAsync(b => b.BillId == id && b.UserId == _userContext.UserId);

        if (bill == null)
        {
            return NotFound();
        }

        _context.Bills.Remove(bill);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id:guid}/relationships")]
    public async Task<ActionResult<BillRelationshipsDto>> GetBillRelationships(Guid id)
    {
        var bill = await _context.Bills
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BillId == id && b.UserId == _userContext.UserId);

        if (bill == null)
        {
            return NotFound();
        }

        // Get linked properties
        var properties = await _context.PropertyBills
            .AsNoTracking()
            .Where(pb => pb.BillId == id)
            .Select(pb => new BillRelatedPropertyDto(
                pb.Property.PropertyId,
                pb.Property.PropertyName,
                pb.Property.PropertyAddress,
                pb.Property.PropertyType
            ))
            .ToListAsync();

        // Get linked providers
        var providers = await _context.BillProviders
            .AsNoTracking()
            .Where(bp => bp.BillId == id)
            .Select(bp => new BillRelatedProviderDto(
                bp.Provider.ProviderId,
                bp.Provider.ProviderName,
                bp.Provider.ProviderType
            ))
            .ToListAsync();

        // Get linked utilities
        var utilities = await _context.BillUtilities
            .AsNoTracking()
            .Where(bu => bu.BillId == id)
            .Select(bu => new BillRelatedUtilityDto(
                bu.Utility.UtilityId,
                bu.Utility.UtilityName,
                bu.Utility.UtilityType
            ))
            .ToListAsync();

        return Ok(new BillRelationshipsDto(properties, providers, utilities));
    }

    private static BillDto MapToDto(Bill bill)
    {
        var utilityName = bill.BillUtilities?.FirstOrDefault()?.Utility?.UtilityName;
        var providerName = bill.BillProviders?.FirstOrDefault()?.Provider?.ProviderName;

        return new BillDto(
            bill.BillId,
            bill.BillReferenceId,
            bill.BillAmount,
            bill.BillStatus,
            bill.BillDescription,
            bill.BillDate,
            bill.BillDueDate,
            bill.BillPaidDate,
            bill.BillPeriodStart,
            bill.BillPeriodEnd,
            bill.BillQuantity,
            bill.BillUnit,
            bill.BillRate,
            bill.BillTax,
            bill.BillTotalAmount,
            bill.BillCurrency,
            bill.BillNotes,
            bill.BillCategory,
            bill.BillExternalId,
            bill.BillIsActive,
            bill.BillCreatedAt,
            bill.BillUpdatedAt,
            utilityName,
            providerName
        );
    }
}
