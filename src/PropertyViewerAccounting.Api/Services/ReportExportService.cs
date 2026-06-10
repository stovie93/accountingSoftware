using System.Globalization;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using PropertyViewerAccounting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PropertyViewerAccounting.Api.Services;

public class ReportExportService
{
    private readonly AppDbContext _context;
    private readonly UserContext _userContext;

    public ReportExportService(AppDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<(byte[] Data, string ContentType, string FileName)> ExportAsync(
        string configuration,
        string format,
        string reportName)
    {
        var config = JsonSerializer.Deserialize<ReportConfiguration>(configuration, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ReportConfiguration();

        // Fetch data based on source type
        var data = await FetchDataAsync(config);

        // Apply sorting
        if (!string.IsNullOrEmpty(config.SortBy))
        {
            data = config.SortDescending == true
                ? data.OrderByDescending(row => row.GetValueOrDefault(config.SortBy)).ToList()
                : data.OrderBy(row => row.GetValueOrDefault(config.SortBy)).ToList();
        }

        // Build output rows with column mappings
        var outputRows = BuildOutputRows(data, config);

        // Generate file based on format
        var sanitizedName = SanitizeFileName(reportName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        if (format.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = GenerateExcel(outputRows, config);
            return (bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{sanitizedName}_{timestamp}.xlsx");
        }
        else
        {
            var bytes = GenerateCsv(outputRows, config);
            return (bytes, "text/csv", $"{sanitizedName}_{timestamp}.csv");
        }
    }

    private async Task<List<Dictionary<string, object?>>> FetchDataAsync(ReportConfiguration config)
    {
        var userId = _userContext.UserId;

        return config.SourceType?.ToLower() switch
        {
            "bills" => await FetchBillsAsync(config, userId),
            "properties" => await FetchPropertiesAsync(config, userId),
            "providers" => await FetchProvidersAsync(config, userId),
            "utilities" => await FetchUtilitiesAsync(config, userId),
            "items" => await FetchItemsAsync(config, userId),
            _ => new List<Dictionary<string, object?>>()
        };
    }

    private async Task<List<Dictionary<string, object?>>> FetchBillsAsync(ReportConfiguration config, Guid userId)
    {
        var query = _context.Bills
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.BillIsActive)
            .Include(b => b.PropertyBills).ThenInclude(pb => pb.Property)
            .Include(b => b.BillProviders).ThenInclude(bp => bp.Provider)
            .Include(b => b.BillUtilities).ThenInclude(bu => bu.Utility)
            .AsQueryable();

        // Apply filters
        if (config.Filters != null)
        {
            if (config.Filters.TryGetValue("status", out var statusObj) && statusObj is JsonElement statusEl)
            {
                var statuses = statusEl.EnumerateArray().Select(s => s.GetString()).ToList();
                if (statuses.Any())
                    query = query.Where(b => statuses.Contains(b.BillStatus));
            }

            if (config.Filters.TryGetValue("dateRange", out var dateRangeObj) && dateRangeObj is JsonElement dateRangeEl)
            {
                if (dateRangeEl.TryGetProperty("start", out var startEl) && DateTime.TryParse(startEl.GetString(), out var start))
                    query = query.Where(b => b.BillDate >= start);
                if (dateRangeEl.TryGetProperty("end", out var endEl) && DateTime.TryParse(endEl.GetString(), out var end))
                    query = query.Where(b => b.BillDate <= end);
            }

            if (config.Filters.TryGetValue("propertyIds", out var propIdsObj) && propIdsObj is JsonElement propIdsEl)
            {
                var propIds = propIdsEl.EnumerateArray()
                    .Select(p => Guid.TryParse(p.GetString(), out var g) ? g : (Guid?)null)
                    .Where(g => g.HasValue)
                    .Select(g => g!.Value)
                    .ToList();
                if (propIds.Any())
                    query = query.Where(b => b.PropertyBills.Any(pb => propIds.Contains(pb.PropertyId)));
            }
        }

        var bills = await query.ToListAsync();

        return bills.Select(b => new Dictionary<string, object?>
        {
            ["billId"] = b.BillId.ToString(),
            ["billReferenceId"] = b.BillReferenceId,
            ["billExternalId"] = b.BillExternalId,
            ["billAmount"] = b.BillAmount,
            ["billTotalAmount"] = b.BillTotalAmount,
            ["billStatus"] = b.BillStatus,
            ["billDate"] = b.BillDate,
            ["billDueDate"] = b.BillDueDate,
            ["billPaidDate"] = b.BillPaidDate,
            ["billDescription"] = b.BillDescription,
            ["billQuantity"] = b.BillQuantity,
            ["billUnit"] = b.BillUnit,
            ["billRate"] = b.BillRate,
            ["billPeriodStart"] = b.BillPeriodStart,
            ["billPeriodEnd"] = b.BillPeriodEnd,
            ["billCategory"] = b.BillCategory,
            ["billTax"] = b.BillTax,
            ["billNotes"] = b.BillNotes,
            ["property.propertyName"] = b.PropertyBills.FirstOrDefault()?.Property?.PropertyName,
            ["property.propertyCode"] = b.PropertyBills.FirstOrDefault()?.Property?.PropertyCode,
            ["property.propertyAddress"] = b.PropertyBills.FirstOrDefault()?.Property?.PropertyAddress,
            ["provider.providerName"] = b.BillProviders.FirstOrDefault()?.Provider?.ProviderName,
            ["provider.providerCode"] = b.BillProviders.FirstOrDefault()?.Provider?.ProviderCode,
            ["utility.utilityName"] = b.BillUtilities.FirstOrDefault()?.Utility?.UtilityName,
            ["utility.utilityType"] = b.BillUtilities.FirstOrDefault()?.Utility?.UtilityType,
        }).ToList();
    }

    private async Task<List<Dictionary<string, object?>>> FetchPropertiesAsync(ReportConfiguration config, Guid userId)
    {
        var query = _context.Properties
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.PropertyIsActive);

        var properties = await query.ToListAsync();

        return properties.Select(p => new Dictionary<string, object?>
        {
            ["propertyId"] = p.PropertyId.ToString(),
            ["propertyClientId"] = p.PropertyClientId,
            ["propertyName"] = p.PropertyName,
            ["propertyAddress"] = p.PropertyAddress,
            ["propertyCity"] = p.PropertyCity,
            ["propertyState"] = p.PropertyState,
            ["propertyZipCode"] = p.PropertyZipCode,
            ["propertyCountry"] = p.PropertyCountry,
            ["propertyType"] = p.PropertyType,
            ["propertyCode"] = p.PropertyCode,
            ["propertyDescription"] = p.PropertyDescription,
            ["propertyUnitCount"] = p.PropertyUnitCount,
            ["propertySquareFootage"] = p.PropertySquareFootage,
        }).ToList();
    }

    private async Task<List<Dictionary<string, object?>>> FetchProvidersAsync(ReportConfiguration config, Guid userId)
    {
        var query = _context.Providers
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.ProviderIsActive);

        var providers = await query.ToListAsync();

        return providers.Select(p => new Dictionary<string, object?>
        {
            ["providerId"] = p.ProviderId.ToString(),
            ["providerAccountId"] = p.ProviderAccountId,
            ["providerName"] = p.ProviderName,
            ["providerCode"] = p.ProviderCode,
            ["providerDescription"] = p.ProviderDescription,
            ["providerType"] = p.ProviderType,
            ["providerAddress"] = p.ProviderAddress,
            ["providerCity"] = p.ProviderCity,
            ["providerState"] = p.ProviderState,
            ["providerZipCode"] = p.ProviderZipCode,
            ["providerCountry"] = p.ProviderCountry,
            ["providerPhone"] = p.ProviderPhone,
            ["providerEmail"] = p.ProviderEmail,
            ["providerWebsite"] = p.ProviderWebsite,
            ["providerContactName"] = p.ProviderContactName,
            ["providerAccountNumber"] = p.ProviderAccountNumber,
        }).ToList();
    }

    private async Task<List<Dictionary<string, object?>>> FetchUtilitiesAsync(ReportConfiguration config, Guid userId)
    {
        var query = _context.Utilities
            .AsNoTracking()
            .Where(u => u.UserId == userId && u.UtilityIsActive);

        var utilities = await query.ToListAsync();

        return utilities.Select(u => new Dictionary<string, object?>
        {
            ["utilityId"] = u.UtilityId.ToString(),
            ["utilityName"] = u.UtilityName,
            ["utilityType"] = u.UtilityType,
            ["utilityCode"] = u.UtilityCode,
            ["utilityDescription"] = u.UtilityDescription,
        }).ToList();
    }

    private async Task<List<Dictionary<string, object?>>> FetchItemsAsync(ReportConfiguration config, Guid userId)
    {
        var baseQuery = _context.Items
            .AsNoTracking()
            .Include(i => i.ItemEntities).ThenInclude(ie => ie.Entity)
            .Where(i => i.UserId == userId)
            .AsQueryable();

        // Apply filters
        if (config.Filters != null)
        {
            if (config.Filters.TryGetValue("dateRange", out var dateRangeObj) && dateRangeObj is JsonElement dateRangeEl)
            {
                if (dateRangeEl.TryGetProperty("start", out var startEl) && DateTime.TryParse(startEl.GetString(), out var start))
                    baseQuery = baseQuery.Where(i => i.Date >= start);
                if (dateRangeEl.TryGetProperty("end", out var endEl) && DateTime.TryParse(endEl.GetString(), out var end))
                    baseQuery = baseQuery.Where(i => i.Date <= end);
            }
        }

        var items = await baseQuery.ToListAsync();

        return items.Select(i => new Dictionary<string, object?>
        {
            ["itemId"] = i.Id.ToString(),
            ["externalId"] = i.ExternalId,
            ["date"] = i.Date,
            ["amount"] = i.Amount,
            ["quantity"] = i.Quantity,
            ["description"] = i.Description,
            ["source"] = i.Source.ToString(),
            ["entities"] = string.Join(", ", i.ItemEntities.Select(ie => ie.Entity?.Name ?? "")),
        }).ToList();
    }

    private List<List<object?>> BuildOutputRows(List<Dictionary<string, object?>> data, ReportConfiguration config)
    {
        var rows = new List<List<object?>>();
        var columnMappings = config.Columns ?? new List<ColumnMapping>();
        var staticColumns = config.StaticColumns ?? new List<StaticColumn>();

        // Build ordered column list based on outputOrder or default order
        var orderedColumns = new List<(string Type, int Index)>();

        if (config.OutputOrder != null && config.OutputOrder.Count > 0)
        {
            // Use custom order
            foreach (var orderKey in config.OutputOrder)
            {
                if (orderKey == "auto" && config.AutoIncrement != null)
                {
                    orderedColumns.Add(("auto", 0));
                }
                else if (orderKey.StartsWith("static:"))
                {
                    if (int.TryParse(orderKey.Substring(7), out var idx) && idx < staticColumns.Count)
                    {
                        orderedColumns.Add(("static", idx));
                    }
                }
                else if (orderKey.StartsWith("data:"))
                {
                    if (int.TryParse(orderKey.Substring(5), out var idx) && idx < columnMappings.Count)
                    {
                        orderedColumns.Add(("data", idx));
                    }
                }
            }
        }
        else
        {
            // Default order: auto -> static -> data
            if (config.AutoIncrement != null)
            {
                orderedColumns.Add(("auto", 0));
            }
            for (int i = 0; i < staticColumns.Count; i++)
            {
                orderedColumns.Add(("static", i));
            }
            for (int i = 0; i < columnMappings.Count; i++)
            {
                orderedColumns.Add(("data", i));
            }
        }

        // Build headers
        var headers = new List<object?>();
        foreach (var (type, idx) in orderedColumns)
        {
            if (type == "auto")
            {
                headers.Add(config.AutoIncrement?.Header ?? "Line");
            }
            else if (type == "static")
            {
                headers.Add(staticColumns[idx].Header);
            }
            else
            {
                headers.Add(columnMappings[idx].Header ?? columnMappings[idx].Source);
            }
        }
        rows.Add(headers);

        // Build data rows
        var lineNumber = config.AutoIncrement?.StartAt ?? 1;
        foreach (var dataRow in data)
        {
            var row = new List<object?>();

            foreach (var (type, idx) in orderedColumns)
            {
                if (type == "auto")
                {
                    var prefix = config.AutoIncrement?.Prefix ?? "";
                    row.Add($"{prefix}{lineNumber}");
                }
                else if (type == "static")
                {
                    row.Add(staticColumns[idx].Value);
                }
                else
                {
                    var col = columnMappings[idx];
                    var value = dataRow.GetValueOrDefault(col.Source);

                    // Apply formatting if specified
                    if (!string.IsNullOrEmpty(col.Format) && value is DateTime dt)
                    {
                        row.Add(dt.ToString(col.Format));
                    }
                    else if (!string.IsNullOrEmpty(col.Format) && value is decimal dec)
                    {
                        row.Add(dec.ToString(col.Format));
                    }
                    else
                    {
                        row.Add(value);
                    }
                }
            }

            lineNumber++;
            rows.Add(row);
        }

        return rows;
    }

    private byte[] GenerateCsv(List<List<object?>> rows, ReportConfiguration config)
    {
        var sb = new StringBuilder();

        foreach (var row in rows)
        {
            var values = row.Select(v =>
            {
                if (v == null) return "";
                var str = v.ToString() ?? "";
                // Escape quotes and wrap in quotes if contains comma or newline
                if (str.Contains(',') || str.Contains('"') || str.Contains('\n'))
                {
                    str = "\"" + str.Replace("\"", "\"\"") + "\"";
                }
                return str;
            });
            sb.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private byte[] GenerateExcel(List<List<object?>> rows, ReportConfiguration config)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Report");

        for (int rowIdx = 0; rowIdx < rows.Count; rowIdx++)
        {
            var row = rows[rowIdx];
            for (int colIdx = 0; colIdx < row.Count; colIdx++)
            {
                var cell = worksheet.Cell(rowIdx + 1, colIdx + 1);
                var value = row[colIdx];

                if (value == null)
                {
                    cell.Value = "";
                }
                else if (value is decimal dec)
                {
                    cell.Value = dec;
                }
                else if (value is int intVal)
                {
                    cell.Value = intVal;
                }
                else if (value is DateTime dt)
                {
                    cell.Value = dt;
                }
                else
                {
                    cell.Value = value.ToString();
                }
            }
        }

        // Style header row
        if (rows.Count > 0)
        {
            var headerRange = worksheet.Range(1, 1, 1, rows[0].Count);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
    }
}

// Configuration classes
public class ReportConfiguration
{
    public string? SourceType { get; set; }
    public Dictionary<string, object>? Filters { get; set; }
    public List<ColumnMapping>? Columns { get; set; }
    public List<StaticColumn>? StaticColumns { get; set; }
    public AutoIncrementConfig? AutoIncrement { get; set; }
    public string? SortBy { get; set; }
    public bool? SortDescending { get; set; }
    public List<string>? OutputOrder { get; set; }
}

public class ColumnMapping
{
    public string Source { get; set; } = "";
    public string? Header { get; set; }
    public string? Format { get; set; }
}

public class StaticColumn
{
    public string Header { get; set; } = "";
    public string Value { get; set; } = "";
}

public class AutoIncrementConfig
{
    public string? Header { get; set; }
    public int StartAt { get; set; } = 1;
    public string? Prefix { get; set; }
}
