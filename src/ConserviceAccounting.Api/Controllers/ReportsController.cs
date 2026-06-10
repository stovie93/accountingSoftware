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
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserContext _userContext;
    private readonly ReportExportService _exportService;

    public ReportsController(AppDbContext context, UserContext userContext, ReportExportService exportService)
    {
        _context = context;
        _userContext = userContext;
        _exportService = exportService;
    }

    [HttpGet]
    public async Task<ActionResult<ReportListResponse>> GetReports(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] ReportType? type = null,
        [FromQuery] string? search = null,
        [FromQuery] bool includeShared = true)
    {
        var query = _context.ReportDefinitions
            .AsNoTracking()
            .Where(r => r.UserId == _userContext.UserId);

        // Filter: own reports OR shared reports in tenant
        if (includeShared)
        {
            query = query.Where(r => r.CreatedById == _userContext.UserId || r.IsShared);
        }
        else
        {
            query = query.Where(r => r.CreatedById == _userContext.UserId);
        }

        if (type.HasValue)
        {
            query = query.Where(r => r.Type == type.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(searchLower));
        }

        var totalCount = await query.CountAsync();

        var reports = await query
            .Include(r => r.CreatedBy)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new ReportListResponse(
            reports.Select(MapToDto).ToList(),
            totalCount,
            page,
            pageSize
        ));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReportDto>> GetReport(Guid id)
    {
        var report = await _context.ReportDefinitions
            .AsNoTracking()
            .Include(r => r.CreatedBy)
            .FirstOrDefaultAsync(r =>
                r.Id == id &&
                r.UserId == _userContext.UserId &&
                (r.CreatedById == _userContext.UserId || r.IsShared));

        if (report == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(report));
    }

    [HttpPost]
    public async Task<ActionResult<ReportDto>> CreateReport([FromBody] CreateReportRequest request)
    {
        var report = new ReportDefinition
        {
            Id = Guid.NewGuid(),
            UserId = _userContext.UserId,
            CreatedById = _userContext.UserId,
            Name = request.Name,
            Type = request.Type,
            Configuration = request.Configuration,
            IsShared = request.IsShared,
            CreatedAt = DateTime.UtcNow
        };

        _context.ReportDefinitions.Add(report);
        await _context.SaveChangesAsync();

        // Reload with CreatedBy for response
        report = await _context.ReportDefinitions
            .Include(r => r.CreatedBy)
            .FirstAsync(r => r.Id == report.Id);

        return CreatedAtAction(nameof(GetReport), new { id = report.Id }, MapToDto(report));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReportDto>> UpdateReport(Guid id, [FromBody] UpdateReportRequest request)
    {
        var report = await _context.ReportDefinitions
            .Include(r => r.CreatedBy)
            .FirstOrDefaultAsync(r =>
                r.Id == id &&
                r.UserId == _userContext.UserId &&
                r.CreatedById == _userContext.UserId); // Only owner can update

        if (report == null)
        {
            return NotFound();
        }

        report.Name = request.Name;
        report.Type = request.Type;
        report.Configuration = request.Configuration;
        report.IsShared = request.IsShared;

        await _context.SaveChangesAsync();

        return Ok(MapToDto(report));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteReport(Guid id)
    {
        var report = await _context.ReportDefinitions
            .FirstOrDefaultAsync(r =>
                r.Id == id &&
                r.UserId == _userContext.UserId &&
                r.CreatedById == _userContext.UserId); // Only owner can delete

        if (report == null)
        {
            return NotFound();
        }

        _context.ReportDefinitions.Remove(report);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id:guid}/duplicate")]
    public async Task<ActionResult<ReportDto>> DuplicateReport(Guid id)
    {
        var original = await _context.ReportDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.Id == id &&
                r.UserId == _userContext.UserId &&
                (r.CreatedById == _userContext.UserId || r.IsShared));

        if (original == null)
        {
            return NotFound();
        }

        var duplicate = new ReportDefinition
        {
            Id = Guid.NewGuid(),
            UserId = _userContext.UserId,
            CreatedById = _userContext.UserId,
            Name = $"{original.Name} (Copy)",
            Type = original.Type,
            Configuration = original.Configuration,
            IsShared = false, // Duplicates start as private
            CreatedAt = DateTime.UtcNow
        };

        _context.ReportDefinitions.Add(duplicate);
        await _context.SaveChangesAsync();

        duplicate = await _context.ReportDefinitions
            .Include(r => r.CreatedBy)
            .FirstAsync(r => r.Id == duplicate.Id);

        return CreatedAtAction(nameof(GetReport), new { id = duplicate.Id }, MapToDto(duplicate));
    }

    [HttpPost("{id:guid}/export")]
    public async Task<IActionResult> ExportReport(Guid id, [FromQuery] string format = "csv")
    {
        var report = await _context.ReportDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.Id == id &&
                r.UserId == _userContext.UserId &&
                (r.CreatedById == _userContext.UserId || r.IsShared));

        if (report == null)
        {
            return NotFound();
        }

        try
        {
            var (data, contentType, fileName) = await _exportService.ExportAsync(
                report.Configuration,
                format,
                report.Name);

            return File(data, contentType, fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Export failed: {ex.Message}" });
        }
    }

    private static ReportDto MapToDto(ReportDefinition report)
    {
        var createdByName = report.CreatedBy != null
            ? $"{report.CreatedBy.UserFirstName} {report.CreatedBy.UserLastName}".Trim()
            : null;

        if (string.IsNullOrEmpty(createdByName) && report.CreatedBy != null)
        {
            createdByName = report.CreatedBy.UserEmail;
        }

        return new ReportDto(
            report.Id,
            report.Name,
            report.Type,
            report.Configuration,
            report.IsShared,
            report.CreatedById,
            createdByName,
            report.CreatedAt
        );
    }
}
