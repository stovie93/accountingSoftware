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
[Route("api/residents")]
[Authorize]
public class ResidentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserContext _userContext;
    private const int BatchSize = 100;

    public ResidentsController(AppDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<ResidentListResponse>> GetResidents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] Guid? propertyId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] bool includeInactive = false)
    {
        var query = _context.Residents
            .AsNoTracking()
            .Where(r => r.UserId == _userContext.UserId);

        // Filter by property via bridge table
        if (propertyId.HasValue)
        {
            query = query.Where(r => r.PropertyResidents.Any(pr => pr.PropertyId == propertyId.Value));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.ResidentStatus == status);
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(r =>
                r.ResidentFirstName.ToLower().Contains(searchLower) ||
                r.ResidentLastName.ToLower().Contains(searchLower) ||
                (r.ResidentEmail != null && r.ResidentEmail.ToLower().Contains(searchLower)) ||
                (r.ResidentExternalId != null && r.ResidentExternalId.ToLower().Contains(searchLower)) ||
                (r.ResidentUnitNumber != null && r.ResidentUnitNumber.ToLower().Contains(searchLower)));
        }

        if (!includeInactive)
        {
            query = query.Where(r => r.ResidentIsActive);
        }

        var totalCount = await query.CountAsync();

        var residents = await query
            .OrderBy(r => r.ResidentLastName)
            .ThenBy(r => r.ResidentFirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new ResidentListResponse(
            residents.Select(MapToDto).ToList(),
            totalCount,
            page,
            pageSize
        ));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResidentDto>> GetResident(Guid id)
    {
        var resident = await _context.Residents
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ResidentId == id && r.UserId == _userContext.UserId);

        if (resident == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(resident));
    }

    [HttpPost]
    public async Task<ActionResult<ResidentDto>> CreateResident([FromBody] CreateResidentRequest request)
    {
        var existingExternalId = await _context.Residents
            .AnyAsync(r => r.UserId == _userContext.UserId && r.ResidentExternalId == request.ResidentExternalId);

        if (existingExternalId)
        {
            return BadRequest(new { message = "A resident with this external ID already exists" });
        }

        var resident = new Resident
        {
            ResidentId = Guid.NewGuid(),
            UserId = _userContext.UserId,
            ResidentExternalId = request.ResidentExternalId,
            ResidentFirstName = request.ResidentFirstName,
            ResidentLastName = request.ResidentLastName,
            ResidentMiddleName = request.ResidentMiddleName,
            ResidentEmail = request.ResidentEmail,
            ResidentPhone = request.ResidentPhone,
            ResidentAlternatePhone = request.ResidentAlternatePhone,
            ResidentUnitNumber = request.ResidentUnitNumber,
            ResidentLeaseStart = request.ResidentLeaseStart,
            ResidentLeaseEnd = request.ResidentLeaseEnd,
            ResidentMonthlyRent = request.ResidentMonthlyRent,
            ResidentAddress = request.ResidentAddress,
            ResidentCity = request.ResidentCity,
            ResidentState = request.ResidentState,
            ResidentZipCode = request.ResidentZipCode,
            ResidentStatus = request.ResidentStatus ?? "Active",
            ResidentType = request.ResidentType,
            ResidentAttributes = request.ResidentAttributes != null
                ? JsonSerializer.Serialize(request.ResidentAttributes)
                : null,
            ResidentIsActive = true,
            ResidentCreatedAt = DateTime.UtcNow,
            ResidentUpdatedAt = DateTime.UtcNow
        };

        _context.Residents.Add(resident);

        // Link to property if provided
        if (request.PropertyId.HasValue)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.PropertyId == request.PropertyId.Value && p.UserId == _userContext.UserId);

            if (property != null)
            {
                _context.PropertyResidents.Add(new PropertyResident
                {
                    PropertyResidentId = Guid.NewGuid(),
                    PropertyId = property.PropertyId,
                    ResidentId = resident.ResidentId,
                    PropertyResidentUnitNumber = request.ResidentUnitNumber,
                    PropertyResidentMoveInDate = request.ResidentLeaseStart,
                    PropertyResidentIsActive = true,
                    PropertyResidentCreatedAt = DateTime.UtcNow,
                    PropertyResidentUpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetResident), new { id = resident.ResidentId }, MapToDto(resident));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ResidentDto>> UpdateResident(Guid id, [FromBody] UpdateResidentRequest request)
    {
        var resident = await _context.Residents
            .FirstOrDefaultAsync(r => r.ResidentId == id && r.UserId == _userContext.UserId);

        if (resident == null)
        {
            return NotFound();
        }

        resident.ResidentFirstName = request.ResidentFirstName;
        resident.ResidentLastName = request.ResidentLastName;
        resident.ResidentMiddleName = request.ResidentMiddleName;
        resident.ResidentEmail = request.ResidentEmail;
        resident.ResidentPhone = request.ResidentPhone;
        resident.ResidentAlternatePhone = request.ResidentAlternatePhone;
        resident.ResidentUnitNumber = request.ResidentUnitNumber;
        resident.ResidentLeaseStart = request.ResidentLeaseStart;
        resident.ResidentLeaseEnd = request.ResidentLeaseEnd;
        resident.ResidentMonthlyRent = request.ResidentMonthlyRent;
        resident.ResidentAddress = request.ResidentAddress;
        resident.ResidentCity = request.ResidentCity;
        resident.ResidentState = request.ResidentState;
        resident.ResidentZipCode = request.ResidentZipCode;
        resident.ResidentStatus = request.ResidentStatus ?? "Active";
        resident.ResidentType = request.ResidentType;
        resident.ResidentAttributes = request.ResidentAttributes != null
            ? JsonSerializer.Serialize(request.ResidentAttributes)
            : null;
        resident.ResidentIsActive = request.ResidentIsActive;
        resident.ResidentUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapToDto(resident));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteResident(Guid id)
    {
        var resident = await _context.Residents
            .FirstOrDefaultAsync(r => r.ResidentId == id && r.UserId == _userContext.UserId);

        if (resident == null)
        {
            return NotFound();
        }

        _context.Residents.Remove(resident);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id:guid}/relationships")]
    public async Task<ActionResult<ResidentRelationshipsDto>> GetResidentRelationships(Guid id)
    {
        var resident = await _context.Residents
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ResidentId == id && r.UserId == _userContext.UserId);

        if (resident == null)
        {
            return NotFound();
        }

        var properties = await _context.PropertyResidents
            .AsNoTracking()
            .Where(pr => pr.ResidentId == id)
            .Select(pr => new ResidentRelatedPropertyDto(
                pr.Property.PropertyId,
                pr.Property.PropertyName,
                pr.Property.PropertyAddress,
                pr.PropertyResidentUnitNumber,
                pr.PropertyResidentMoveInDate,
                pr.PropertyResidentMoveOutDate
            ))
            .ToListAsync();

        return Ok(new ResidentRelationshipsDto(properties));
    }

    /// <summary>
    /// Bulk import residents for a specific property.
    /// Supports upsert: creates new residents or updates existing ones by ExternalId.
    /// </summary>
    [HttpPost("import")]
    public async Task<ActionResult<ResidentImportResponse>> ImportResidents([FromBody] BulkImportResidentsRequest request)
    {
        // Validate property belongs to user
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.PropertyId == request.PropertyId && p.UserId == _userContext.UserId);

        if (property == null)
        {
            return BadRequest(new { message = "Property not found or access denied" });
        }

        if (request.Residents == null || request.Residents.Count == 0)
        {
            return BadRequest(new { message = "No residents provided" });
        }

        var batchId = Guid.NewGuid();
        var results = new ResidentImportResponse
        {
            BatchId = batchId,
            PropertyId = request.PropertyId,
            TotalRequested = request.Residents.Count,
            Created = 0,
            Updated = 0,
            Failed = 0,
            Errors = new List<ResidentImportError>()
        };

        // Get existing residents by external ID for upsert
        var externalIds = request.Residents.Select(r => r.ExternalId).Where(id => !string.IsNullOrEmpty(id)).ToList();
        var existingResidents = await _context.Residents
            .Where(r => r.UserId == _userContext.UserId && externalIds.Contains(r.ResidentExternalId))
            .ToDictionaryAsync(r => r.ResidentExternalId);

        // Process in batches
        var batches = request.Residents
            .Select((item, index) => new { Item = item, Index = index })
            .GroupBy(x => x.Index / BatchSize)
            .Select(g => g.Select(x => new { x.Item, x.Index }).ToList())
            .ToList();

        foreach (var batch in batches)
        {
            var residentsToAdd = new List<Resident>();
            var propertyResidentsToAdd = new List<PropertyResident>();

            foreach (var entry in batch)
            {
                try
                {
                    // Validation
                    if (string.IsNullOrWhiteSpace(entry.Item.ExternalId))
                    {
                        throw new Exception("ExternalId is required");
                    }
                    if (string.IsNullOrWhiteSpace(entry.Item.FirstName))
                    {
                        throw new Exception("FirstName is required");
                    }
                    if (string.IsNullOrWhiteSpace(entry.Item.LastName))
                    {
                        throw new Exception("LastName is required");
                    }

                    if (existingResidents.TryGetValue(entry.Item.ExternalId, out var existingResident))
                    {
                        // Update existing
                        if (request.UpdateExisting)
                        {
                            existingResident.ResidentFirstName = entry.Item.FirstName;
                            existingResident.ResidentLastName = entry.Item.LastName;
                            existingResident.ResidentMiddleName = entry.Item.MiddleName;
                            existingResident.ResidentEmail = entry.Item.Email;
                            existingResident.ResidentPhone = entry.Item.Phone;
                            existingResident.ResidentAlternatePhone = entry.Item.AlternatePhone;
                            existingResident.ResidentUnitNumber = entry.Item.UnitNumber;
                            existingResident.ResidentLeaseStart = entry.Item.LeaseStart;
                            existingResident.ResidentLeaseEnd = entry.Item.LeaseEnd;
                            existingResident.ResidentMonthlyRent = entry.Item.MonthlyRent;
                            existingResident.ResidentAddress = entry.Item.Address;
                            existingResident.ResidentCity = entry.Item.City;
                            existingResident.ResidentState = entry.Item.State;
                            existingResident.ResidentZipCode = entry.Item.ZipCode;
                            existingResident.ResidentStatus = entry.Item.Status ?? "Active";
                            existingResident.ResidentType = entry.Item.Type;
                            existingResident.ResidentAttributes = entry.Item.Attributes != null
                                ? JsonSerializer.Serialize(entry.Item.Attributes)
                                : null;
                            existingResident.ResidentBatchId = batchId;
                            existingResident.ResidentUpdatedAt = DateTime.UtcNow;
                            results.Updated++;
                        }
                    }
                    else
                    {
                        // Create new
                        var resident = new Resident
                        {
                            ResidentId = Guid.NewGuid(),
                            UserId = _userContext.UserId,
                            ResidentExternalId = entry.Item.ExternalId,
                            ResidentFirstName = entry.Item.FirstName,
                            ResidentLastName = entry.Item.LastName,
                            ResidentMiddleName = entry.Item.MiddleName,
                            ResidentEmail = entry.Item.Email,
                            ResidentPhone = entry.Item.Phone,
                            ResidentAlternatePhone = entry.Item.AlternatePhone,
                            ResidentUnitNumber = entry.Item.UnitNumber,
                            ResidentLeaseStart = entry.Item.LeaseStart,
                            ResidentLeaseEnd = entry.Item.LeaseEnd,
                            ResidentMonthlyRent = entry.Item.MonthlyRent,
                            ResidentAddress = entry.Item.Address,
                            ResidentCity = entry.Item.City,
                            ResidentState = entry.Item.State,
                            ResidentZipCode = entry.Item.ZipCode,
                            ResidentStatus = entry.Item.Status ?? "Active",
                            ResidentType = entry.Item.Type,
                            ResidentAttributes = entry.Item.Attributes != null
                                ? JsonSerializer.Serialize(entry.Item.Attributes)
                                : null,
                            ResidentBatchId = batchId,
                            ResidentIsActive = true,
                            ResidentCreatedAt = DateTime.UtcNow,
                            ResidentUpdatedAt = DateTime.UtcNow
                        };

                        residentsToAdd.Add(resident);

                        // Create property link
                        propertyResidentsToAdd.Add(new PropertyResident
                        {
                            PropertyResidentId = Guid.NewGuid(),
                            PropertyId = request.PropertyId,
                            ResidentId = resident.ResidentId,
                            PropertyResidentUnitNumber = entry.Item.UnitNumber,
                            PropertyResidentMoveInDate = entry.Item.LeaseStart,
                            PropertyResidentIsActive = true,
                            PropertyResidentCreatedAt = DateTime.UtcNow,
                            PropertyResidentUpdatedAt = DateTime.UtcNow
                        });

                        // Track for duplicate detection within same import
                        existingResidents[entry.Item.ExternalId] = resident;
                        results.Created++;
                    }
                }
                catch (Exception ex)
                {
                    results.Failed++;
                    results.Errors.Add(new ResidentImportError
                    {
                        Index = entry.Index,
                        ExternalId = entry.Item.ExternalId,
                        Message = ex.Message
                    });
                }
            }

            // Bulk insert the batch
            if (residentsToAdd.Count > 0)
            {
                await _context.Residents.AddRangeAsync(residentsToAdd);
                await _context.PropertyResidents.AddRangeAsync(propertyResidentsToAdd);
            }

            await _context.SaveChangesAsync();
        }

        return Ok(results);
    }

    /// <summary>
    /// Get residents by batch ID for tracking import results.
    /// </summary>
    [HttpGet("batch/{batchId:guid}")]
    public async Task<ActionResult<ResidentListResponse>> GetBatchResidents(
        Guid batchId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.Residents
            .AsNoTracking()
            .Where(r => r.UserId == _userContext.UserId && r.ResidentBatchId == batchId);

        var totalCount = await query.CountAsync();

        var residents = await query
            .OrderBy(r => r.ResidentLastName)
            .ThenBy(r => r.ResidentFirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new ResidentListResponse(
            residents.Select(MapToDto).ToList(),
            totalCount,
            page,
            pageSize
        ));
    }

    /// <summary>
    /// Push residents to PropertyViewer environment (mock implementation).
    /// </summary>
    [HttpPost("push")]
    public async Task<ActionResult<ResidentPushResponse>> PushToPropertyViewer([FromBody] ResidentPushRequest request)
    {
        // Validate property belongs to user
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.PropertyId == request.PropertyId && p.UserId == _userContext.UserId);

        if (property == null)
        {
            return BadRequest(new { message = "Property not found or access denied" });
        }

        // Get residents to push
        var residentsQuery = _context.Residents
            .Where(r => r.UserId == _userContext.UserId && r.ResidentIsActive)
            .Where(r => r.PropertyResidents.Any(pr => pr.PropertyId == request.PropertyId));

        if (request.ResidentIds != null && request.ResidentIds.Count > 0)
        {
            residentsQuery = residentsQuery.Where(r => request.ResidentIds.Contains(r.ResidentId));
        }

        var residentCount = await residentsQuery.CountAsync();

        if (residentCount == 0)
        {
            return BadRequest(new { message = "No residents found to push" });
        }

        // Mock implementation - in production, this would call the PropertyViewer API
        var pushId = Guid.NewGuid();

        // Log the push (could also create a DataPush record for tracking)
        Console.WriteLine($"[PropertyViewer Push] PushId: {pushId}, PropertyId: {request.PropertyId}, ResidentCount: {residentCount}");

        return Ok(new ResidentPushResponse
        {
            PushId = pushId,
            PropertyId = request.PropertyId,
            ResidentCount = residentCount,
            Status = "Completed",
            PushedAt = DateTime.UtcNow
        });
    }

    private static ResidentDto MapToDto(Resident resident)
    {
        Dictionary<string, object>? attributes = null;
        if (!string.IsNullOrEmpty(resident.ResidentAttributes))
        {
            attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(resident.ResidentAttributes);
        }

        return new ResidentDto(
            resident.ResidentId,
            resident.ResidentExternalId,
            resident.ResidentFirstName,
            resident.ResidentLastName,
            resident.ResidentMiddleName,
            resident.ResidentEmail,
            resident.ResidentPhone,
            resident.ResidentAlternatePhone,
            resident.ResidentUnitNumber,
            resident.ResidentLeaseStart,
            resident.ResidentLeaseEnd,
            resident.ResidentMonthlyRent,
            resident.ResidentAddress,
            resident.ResidentCity,
            resident.ResidentState,
            resident.ResidentZipCode,
            resident.ResidentStatus,
            resident.ResidentType,
            attributes,
            resident.ResidentBatchId,
            resident.ResidentIsActive,
            resident.ResidentCreatedAt,
            resident.ResidentUpdatedAt
        );
    }
}
