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
[Route("api/integrations")]
[Authorize]
public class IntegrationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserContext _userContext;

    public IntegrationsController(AppDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    #region Accounting Software

    [HttpGet("software")]
    public async Task<ActionResult<AccountingSoftwareListResponse>> GetAccountingSoftware(
        [FromQuery] bool activeOnly = true)
    {
        var query = _context.AccountingSoftware.AsNoTracking();

        if (activeOnly)
        {
            query = query.Where(s => s.IsActive);
        }

        var software = await query
            .OrderBy(s => s.Name)
            .ToListAsync();

        return Ok(new AccountingSoftwareListResponse(
            software.Select(s => new AccountingSoftwareDto(
                s.Id,
                s.Name,
                s.Code,
                s.Description,
                s.ConnectionType,
                s.LogoUrl,
                s.IsActive
            )).ToList(),
            software.Count
        ));
    }

    [HttpGet("software/{id:guid}")]
    public async Task<ActionResult<AccountingSoftwareDto>> GetAccountingSoftwareById(Guid id)
    {
        var software = await _context.AccountingSoftware
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (software == null)
        {
            return NotFound();
        }

        return Ok(new AccountingSoftwareDto(
            software.Id,
            software.Name,
            software.Code,
            software.Description,
            software.ConnectionType,
            software.LogoUrl,
            software.IsActive
        ));
    }

    #endregion

    #region Client Accounting Connections

    [HttpGet("connections")]
    public async Task<ActionResult<ConnectionListResponse>> GetConnections(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] Guid? softwareId = null,
        [FromQuery] bool? isActive = null)
    {
        var query = _context.ClientAccountingConnections
            .AsNoTracking()
            .Include(c => c.AccountingSoftware)
            .Where(c => c.UserId == _userContext.UserId);

        if (softwareId.HasValue)
        {
            query = query.Where(c => c.AccountingSoftwareId == softwareId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var connections = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new ConnectionListResponse(
            connections.Select(MapToConnectionDto).ToList(),
            totalCount,
            page,
            pageSize
        ));
    }

    [HttpGet("connections/{id:guid}")]
    public async Task<ActionResult<ClientAccountingConnectionDetailDto>> GetConnection(Guid id)
    {
        var connection = await _context.ClientAccountingConnections
            .AsNoTracking()
            .Include(c => c.AccountingSoftware)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == _userContext.UserId);

        if (connection == null)
        {
            return NotFound();
        }

        return Ok(MapToConnectionDetailDto(connection));
    }

    [HttpPost("connections")]
    public async Task<ActionResult<ClientAccountingConnectionDetailDto>> CreateConnection(
        [FromBody] CreateConnectionRequest request)
    {
        var software = await _context.AccountingSoftware
            .FirstOrDefaultAsync(s => s.Id == request.AccountingSoftwareId);

        if (software == null)
        {
            return BadRequest(new { message = "Invalid accounting software ID" });
        }

        var connection = new ClientAccountingConnection
        {
            Id = Guid.NewGuid(),
            UserId = _userContext.UserId,
            AccountingSoftwareId = request.AccountingSoftwareId,
            Name = request.Name,
            ConnectionString = request.ConnectionString,
            DatabaseName = request.DatabaseName,
            DatabaseServer = request.DatabaseServer,
            DatabaseUsername = request.DatabaseUsername,
            DatabasePassword = request.DatabasePassword,
            ApiEndpoint = request.ApiEndpoint,
            ApiKey = request.ApiKey,
            ApiSecret = request.ApiSecret,
            SftpHost = request.SftpHost,
            SftpPort = request.SftpPort,
            SftpUsername = request.SftpUsername,
            SftpPassword = request.SftpPassword,
            SftpPath = request.SftpPath,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ClientAccountingConnections.Add(connection);
        await _context.SaveChangesAsync();

        // Reload with navigation property
        connection = await _context.ClientAccountingConnections
            .Include(c => c.AccountingSoftware)
            .FirstAsync(c => c.Id == connection.Id);

        return CreatedAtAction(nameof(GetConnection), new { id = connection.Id }, MapToConnectionDetailDto(connection));
    }

    [HttpPut("connections/{id:guid}")]
    public async Task<ActionResult<ClientAccountingConnectionDetailDto>> UpdateConnection(
        Guid id,
        [FromBody] UpdateConnectionRequest request)
    {
        var connection = await _context.ClientAccountingConnections
            .Include(c => c.AccountingSoftware)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == _userContext.UserId);

        if (connection == null)
        {
            return NotFound();
        }

        connection.Name = request.Name;
        connection.ConnectionString = request.ConnectionString;
        connection.DatabaseName = request.DatabaseName;
        connection.DatabaseServer = request.DatabaseServer;
        connection.DatabaseUsername = request.DatabaseUsername;
        if (!string.IsNullOrEmpty(request.DatabasePassword))
        {
            connection.DatabasePassword = request.DatabasePassword;
        }
        connection.ApiEndpoint = request.ApiEndpoint;
        if (!string.IsNullOrEmpty(request.ApiKey))
        {
            connection.ApiKey = request.ApiKey;
        }
        if (!string.IsNullOrEmpty(request.ApiSecret))
        {
            connection.ApiSecret = request.ApiSecret;
        }
        connection.SftpHost = request.SftpHost;
        connection.SftpPort = request.SftpPort;
        connection.SftpUsername = request.SftpUsername;
        if (!string.IsNullOrEmpty(request.SftpPassword))
        {
            connection.SftpPassword = request.SftpPassword;
        }
        connection.SftpPath = request.SftpPath;
        connection.IsActive = request.IsActive;
        connection.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapToConnectionDetailDto(connection));
    }

    [HttpDelete("connections/{id:guid}")]
    public async Task<IActionResult> DeleteConnection(Guid id)
    {
        var connection = await _context.ClientAccountingConnections
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == _userContext.UserId);

        if (connection == null)
        {
            return NotFound();
        }

        // Check if any schedules use this connection
        var hasSchedules = await _context.ReportSchedules
            .AnyAsync(s => s.ConnectionId == id);

        if (hasSchedules)
        {
            return BadRequest(new { message = "Cannot delete connection that is used by report schedules" });
        }

        _context.ClientAccountingConnections.Remove(connection);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("connections/{id:guid}/test")]
    public async Task<ActionResult<TestConnectionResult>> TestConnection(Guid id)
    {
        var connection = await _context.ClientAccountingConnections
            .Include(c => c.AccountingSoftware)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == _userContext.UserId);

        if (connection == null)
        {
            return NotFound();
        }

        // Simulate connection test (in real implementation, this would actually test the connection)
        var success = true;
        var message = "Connection successful";

        // Update connection with test results
        connection.LastTestedAt = DateTime.UtcNow;
        connection.LastTestStatus = success ? "Success" : "Failed";
        connection.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new TestConnectionResult(success, message, connection.LastTestedAt.Value));
    }

    #endregion

    #region Report Schedules

    [HttpGet("schedules")]
    public async Task<ActionResult<ScheduleListResponse>> GetSchedules(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] Guid? reportId = null,
        [FromQuery] Guid? connectionId = null,
        [FromQuery] bool? isActive = null)
    {
        var query = _context.ReportSchedules
            .AsNoTracking()
            .Include(s => s.ReportDefinition)
            .Include(s => s.Connection)
                .ThenInclude(c => c.AccountingSoftware)
            .Where(s => s.UserId == _userContext.UserId);

        if (reportId.HasValue)
        {
            query = query.Where(s => s.ReportDefinitionId == reportId.Value);
        }

        if (connectionId.HasValue)
        {
            query = query.Where(s => s.ConnectionId == connectionId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var schedules = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new ScheduleListResponse(
            schedules.Select(MapToScheduleDto).ToList(),
            totalCount,
            page,
            pageSize
        ));
    }

    [HttpGet("schedules/{id:guid}")]
    public async Task<ActionResult<ReportScheduleDto>> GetSchedule(Guid id)
    {
        var schedule = await _context.ReportSchedules
            .AsNoTracking()
            .Include(s => s.ReportDefinition)
            .Include(s => s.Connection)
                .ThenInclude(c => c.AccountingSoftware)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == _userContext.UserId);

        if (schedule == null)
        {
            return NotFound();
        }

        return Ok(MapToScheduleDto(schedule));
    }

    [HttpPost("schedules")]
    public async Task<ActionResult<ReportScheduleDto>> CreateSchedule([FromBody] CreateScheduleRequest request)
    {
        // Validate report definition exists and belongs to tenant
        var report = await _context.ReportDefinitions
            .FirstOrDefaultAsync(r => r.Id == request.ReportDefinitionId && r.UserId == _userContext.UserId);

        if (report == null)
        {
            return BadRequest(new { message = "Invalid report definition ID" });
        }

        // Validate connection exists and belongs to tenant
        var connection = await _context.ClientAccountingConnections
            .FirstOrDefaultAsync(c => c.Id == request.ConnectionId && c.UserId == _userContext.UserId);

        if (connection == null)
        {
            return BadRequest(new { message = "Invalid connection ID" });
        }

        var schedule = new ReportSchedule
        {
            Id = Guid.NewGuid(),
            UserId = _userContext.UserId,
            ReportDefinitionId = request.ReportDefinitionId,
            ConnectionId = request.ConnectionId,
            Name = request.Name,
            Description = request.Description,
            Frequency = request.Frequency,
            CronExpression = request.CronExpression,
            TimeOfDay = request.TimeOfDay ?? new TimeOnly(0, 0),
            DayOfWeek = request.DayOfWeek,
            DayOfMonth = request.DayOfMonth,
            StartDate = request.StartDate ?? DateTime.UtcNow.Date,
            EndDate = request.EndDate,
            ExportFormat = request.ExportFormat,
            DestinationTable = request.DestinationTable,
            DestinationPath = request.DestinationPath,
            IsActive = request.IsActive,
            NextRunAt = CalculateNextRunTime(request),
            CreatedById = _userContext.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ReportSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        schedule = await _context.ReportSchedules
            .Include(s => s.ReportDefinition)
            .Include(s => s.Connection)
                .ThenInclude(c => c.AccountingSoftware)
            .FirstAsync(s => s.Id == schedule.Id);

        return CreatedAtAction(nameof(GetSchedule), new { id = schedule.Id }, MapToScheduleDto(schedule));
    }

    [HttpPut("schedules/{id:guid}")]
    public async Task<ActionResult<ReportScheduleDto>> UpdateSchedule(Guid id, [FromBody] UpdateScheduleRequest request)
    {
        var schedule = await _context.ReportSchedules
            .Include(s => s.ReportDefinition)
            .Include(s => s.Connection)
                .ThenInclude(c => c.AccountingSoftware)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == _userContext.UserId);

        if (schedule == null)
        {
            return NotFound();
        }

        schedule.Name = request.Name;
        schedule.Description = request.Description;
        schedule.Frequency = request.Frequency;
        schedule.CronExpression = request.CronExpression;
        schedule.TimeOfDay = request.TimeOfDay ?? schedule.TimeOfDay;
        schedule.DayOfWeek = request.DayOfWeek;
        schedule.DayOfMonth = request.DayOfMonth;
        schedule.StartDate = request.StartDate ?? schedule.StartDate;
        schedule.EndDate = request.EndDate;
        schedule.ExportFormat = request.ExportFormat;
        schedule.DestinationTable = request.DestinationTable;
        schedule.DestinationPath = request.DestinationPath;
        schedule.IsActive = request.IsActive;
        schedule.NextRunAt = CalculateNextRunTimeFromSchedule(schedule);
        schedule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapToScheduleDto(schedule));
    }

    [HttpDelete("schedules/{id:guid}")]
    public async Task<IActionResult> DeleteSchedule(Guid id)
    {
        var schedule = await _context.ReportSchedules
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == _userContext.UserId);

        if (schedule == null)
        {
            return NotFound();
        }

        _context.ReportSchedules.Remove(schedule);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("schedules/{id:guid}/toggle")]
    public async Task<ActionResult<ReportScheduleDto>> ToggleSchedule(Guid id)
    {
        var schedule = await _context.ReportSchedules
            .Include(s => s.ReportDefinition)
            .Include(s => s.Connection)
                .ThenInclude(c => c.AccountingSoftware)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == _userContext.UserId);

        if (schedule == null)
        {
            return NotFound();
        }

        schedule.IsActive = !schedule.IsActive;
        schedule.UpdatedAt = DateTime.UtcNow;

        if (schedule.IsActive)
        {
            schedule.NextRunAt = CalculateNextRunTimeFromSchedule(schedule);
        }

        await _context.SaveChangesAsync();

        return Ok(MapToScheduleDto(schedule));
    }

    [HttpPost("schedules/{id:guid}/run")]
    public async Task<ActionResult<ReportScheduleRunDto>> TriggerScheduleRun(Guid id, [FromBody] TriggerScheduleRunRequest? request = null)
    {
        var schedule = await _context.ReportSchedules
            .Include(s => s.ReportDefinition)
            .Include(s => s.Connection)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == _userContext.UserId);

        if (schedule == null)
        {
            return NotFound();
        }

        // Create a new run record
        var run = new ReportScheduleRun
        {
            Id = Guid.NewGuid(),
            ScheduleId = schedule.Id,
            Status = ScheduleRunStatus.Pending,
            StartedAt = DateTime.UtcNow,
            RecordsProcessed = 0,
            RecordsFailed = 0
        };

        _context.ReportScheduleRuns.Add(run);
        await _context.SaveChangesAsync();

        // In a real implementation, this would queue a background job to execute the report
        // For now, simulate a quick execution
        run.Status = ScheduleRunStatus.Success;
        run.CompletedAt = DateTime.UtcNow;
        run.RecordsProcessed = new Random().Next(50, 200);

        schedule.LastRunAt = run.CompletedAt;
        schedule.LastRunStatus = "Success";
        schedule.NextRunAt = CalculateNextRunTimeFromSchedule(schedule);

        await _context.SaveChangesAsync();

        return Ok(new ReportScheduleRunDto(
            run.Id,
            run.ScheduleId,
            run.Status,
            run.StartedAt,
            run.CompletedAt,
            run.RecordsProcessed,
            run.RecordsFailed,
            run.ErrorMessage,
            run.OutputFilePath,
            run.FileSizeBytes
        ));
    }

    #endregion

    #region Schedule Run History

    [HttpGet("schedules/{id:guid}/runs")]
    public async Task<ActionResult<ScheduleRunListResponse>> GetScheduleRuns(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var schedule = await _context.ReportSchedules
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == _userContext.UserId);

        if (schedule == null)
        {
            return NotFound();
        }

        var query = _context.ReportScheduleRuns
            .AsNoTracking()
            .Where(r => r.ScheduleId == id);

        var totalCount = await query.CountAsync();

        var runs = await query
            .OrderByDescending(r => r.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new ScheduleRunListResponse(
            runs.Select(r => new ReportScheduleRunDto(
                r.Id,
                r.ScheduleId,
                r.Status,
                r.StartedAt,
                r.CompletedAt,
                r.RecordsProcessed,
                r.RecordsFailed,
                r.ErrorMessage,
                r.OutputFilePath,
                r.FileSizeBytes
            )).ToList(),
            totalCount,
            page,
            pageSize
        ));
    }

    [HttpGet("runs/{id:guid}")]
    public async Task<ActionResult<ReportScheduleRunDto>> GetRun(Guid id)
    {
        var run = await _context.ReportScheduleRuns
            .AsNoTracking()
            .Include(r => r.Schedule)
            .FirstOrDefaultAsync(r => r.Id == id && r.Schedule.UserId == _userContext.UserId);

        if (run == null)
        {
            return NotFound();
        }

        return Ok(new ReportScheduleRunDto(
            run.Id,
            run.ScheduleId,
            run.Status,
            run.StartedAt,
            run.CompletedAt,
            run.RecordsProcessed,
            run.RecordsFailed,
            run.ErrorMessage,
            run.OutputFilePath,
            run.FileSizeBytes
        ));
    }

    #endregion

    #region Helper Methods

    private static ClientAccountingConnectionDto MapToConnectionDto(ClientAccountingConnection connection)
    {
        return new ClientAccountingConnectionDto(
            connection.Id,
            connection.AccountingSoftwareId,
            connection.AccountingSoftware?.Name ?? "Unknown",
            connection.Name,
            connection.AccountingSoftware?.ConnectionType ?? "Unknown",
            connection.IsActive,
            connection.LastTestedAt,
            connection.LastTestStatus,
            connection.CreatedAt,
            connection.UpdatedAt
        );
    }

    private static ClientAccountingConnectionDetailDto MapToConnectionDetailDto(ClientAccountingConnection connection)
    {
        return new ClientAccountingConnectionDetailDto(
            connection.Id,
            connection.AccountingSoftwareId,
            connection.AccountingSoftware?.Name ?? "Unknown",
            connection.Name,
            connection.DatabaseServer,
            connection.DatabaseName,
            connection.DatabaseUsername,
            !string.IsNullOrEmpty(connection.DatabasePassword),
            connection.ApiEndpoint,
            !string.IsNullOrEmpty(connection.ApiKey),
            !string.IsNullOrEmpty(connection.ApiSecret),
            connection.SftpHost,
            connection.SftpPort,
            connection.SftpUsername,
            !string.IsNullOrEmpty(connection.SftpPassword),
            connection.SftpPath,
            connection.IsActive,
            connection.LastTestedAt,
            connection.LastTestStatus,
            connection.CreatedAt,
            connection.UpdatedAt
        );
    }

    private static ReportScheduleDto MapToScheduleDto(ReportSchedule schedule)
    {
        return new ReportScheduleDto(
            schedule.Id,
            schedule.ReportDefinitionId,
            schedule.ReportDefinition?.Name ?? "Unknown Report",
            schedule.ConnectionId,
            schedule.Connection?.Name ?? "Unknown Connection",
            schedule.Connection?.AccountingSoftware?.Name ?? "Unknown Software",
            schedule.Name,
            schedule.Description,
            schedule.Frequency,
            schedule.CronExpression,
            schedule.TimeOfDay,
            schedule.DayOfWeek,
            schedule.DayOfMonth,
            schedule.StartDate,
            schedule.EndDate,
            schedule.ExportFormat,
            schedule.DestinationTable,
            schedule.DestinationPath,
            schedule.IsActive,
            schedule.LastRunAt,
            schedule.LastRunStatus,
            schedule.NextRunAt,
            schedule.CreatedAt
        );
    }

    private static DateTime? CalculateNextRunTime(CreateScheduleRequest request)
    {
        if (!request.IsActive)
            return null;

        var now = DateTime.UtcNow;
        var timeOfDay = request.TimeOfDay ?? new TimeOnly(0, 0);
        var dayOfWeek = request.DayOfWeek.HasValue ? (System.DayOfWeek)request.DayOfWeek.Value : System.DayOfWeek.Monday;

        return request.Frequency switch
        {
            ScheduleFrequency.Once => request.StartDate ?? now.AddDays(1).Date.Add(timeOfDay.ToTimeSpan()),
            ScheduleFrequency.Daily => now.Date.AddDays(1).Add(timeOfDay.ToTimeSpan()),
            ScheduleFrequency.Weekly => GetNextWeekday(dayOfWeek, timeOfDay),
            ScheduleFrequency.BiWeekly => GetNextWeekday(dayOfWeek, timeOfDay).AddDays(7),
            ScheduleFrequency.Monthly => GetNextMonthDay(request.DayOfMonth ?? 1, timeOfDay),
            ScheduleFrequency.Quarterly => GetNextQuarterDay(request.DayOfMonth ?? 1, timeOfDay),
            _ => now.AddDays(1)
        };
    }

    private static DateTime? CalculateNextRunTimeFromSchedule(ReportSchedule schedule)
    {
        if (!schedule.IsActive)
            return null;

        var now = DateTime.UtcNow;
        var timeOfDay = schedule.TimeOfDay;
        var dayOfWeek = schedule.DayOfWeek.HasValue ? (System.DayOfWeek)schedule.DayOfWeek.Value : System.DayOfWeek.Monday;

        return schedule.Frequency switch
        {
            ScheduleFrequency.Once => schedule.StartDate,
            ScheduleFrequency.Daily => now.Date.AddDays(1).Add(timeOfDay.ToTimeSpan()),
            ScheduleFrequency.Weekly => GetNextWeekday(dayOfWeek, timeOfDay),
            ScheduleFrequency.BiWeekly => GetNextWeekday(dayOfWeek, timeOfDay).AddDays(7),
            ScheduleFrequency.Monthly => GetNextMonthDay(schedule.DayOfMonth ?? 1, timeOfDay),
            ScheduleFrequency.Quarterly => GetNextQuarterDay(schedule.DayOfMonth ?? 1, timeOfDay),
            _ => now.AddDays(1)
        };
    }

    private static DateTime GetNextWeekday(System.DayOfWeek dayOfWeek, TimeOnly timeOfDay)
    {
        var today = DateTime.UtcNow.Date;
        var daysUntil = ((int)dayOfWeek - (int)today.DayOfWeek + 7) % 7;
        if (daysUntil == 0) daysUntil = 7;
        return today.AddDays(daysUntil).Add(timeOfDay.ToTimeSpan());
    }

    private static DateTime GetNextMonthDay(int dayOfMonth, TimeOnly timeOfDay)
    {
        var now = DateTime.UtcNow;
        var thisMonth = new DateTime(now.Year, now.Month, Math.Min(dayOfMonth, DateTime.DaysInMonth(now.Year, now.Month)));
        thisMonth = thisMonth.Add(timeOfDay.ToTimeSpan());

        if (thisMonth <= now)
        {
            var nextMonth = now.AddMonths(1);
            return new DateTime(nextMonth.Year, nextMonth.Month, Math.Min(dayOfMonth, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month)))
                .Add(timeOfDay.ToTimeSpan());
        }

        return thisMonth;
    }

    private static DateTime GetNextQuarterDay(int dayOfMonth, TimeOnly timeOfDay)
    {
        var now = DateTime.UtcNow;
        var quarterStartMonths = new[] { 1, 4, 7, 10 };
        var currentQuarter = (now.Month - 1) / 3;
        var nextQuarterMonth = quarterStartMonths[(currentQuarter + 1) % 4];
        var nextQuarterYear = currentQuarter == 3 ? now.Year + 1 : now.Year;

        return new DateTime(nextQuarterYear, nextQuarterMonth, Math.Min(dayOfMonth, DateTime.DaysInMonth(nextQuarterYear, nextQuarterMonth)))
            .Add(timeOfDay.ToTimeSpan());
    }

    #endregion
}
