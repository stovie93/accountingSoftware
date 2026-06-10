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
[Route("api/mock-sync")]
[Authorize]
public class MockSyncController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserContext _userContext;

    public MockSyncController(AppDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    /// <summary>
    /// Simulates a data sync by creating mock utility bills.
    /// This is for demo purposes only - generates random items linked to existing entities.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MockSyncResponse>> TriggerMockSync()
    {
        var userId = _userContext.UserId;

        // Get existing entities to link to
        var entities = await _context.Entities
            .Where(e => e.UserId == userId && e.IsActive)
            .Include(e => e.EntityType)
            .ToListAsync();

        if (entities.Count == 0)
        {
            return BadRequest(new { message = "No entities found to link items to" });
        }

        // Group entities by type for realistic linking
        var propertyEntities = entities.Where(e => e.EntityType?.Code == "property").ToList();
        var utilityEntities = entities.Where(e => e.EntityType?.Code == "utility-type").ToList();
        var vendorEntities = entities.Where(e => e.EntityType?.Code == "vendor").ToList();

        var random = new Random();
        var batchId = Guid.NewGuid();
        var syncedItems = new List<Item>();
        var currentDate = DateTime.UtcNow;

        // Mock utility types with their typical vendors
        var mockBills = new[]
        {
            new { Utility = "Electric", Vendor = "Denver Power & Light", MinAmount = 150m, MaxAmount = 450m, Unit = "kWh" },
            new { Utility = "Water", Vendor = "Denver Water Authority", MinAmount = 80m, MaxAmount = 200m, Unit = "gallons" },
            new { Utility = "Gas", Vendor = "Colorado Natural Gas", MinAmount = 60m, MaxAmount = 180m, Unit = "therms" },
            new { Utility = "Sewer", Vendor = "Denver Metro Wastewater", MinAmount = 40m, MaxAmount = 100m, Unit = (string?)null },
            new { Utility = "Trash", Vendor = "EcoWaste Services", MinAmount = 75m, MaxAmount = 150m, Unit = "pickups" },
        };

        // Generate 3-5 random mock items
        var itemCount = random.Next(3, 6);

        for (int i = 0; i < itemCount; i++)
        {
            var mockBill = mockBills[random.Next(mockBills.Length)];
            var property = propertyEntities.Count > 0
                ? propertyEntities[random.Next(propertyEntities.Count)]
                : entities[random.Next(entities.Count)];

            var amount = Math.Round((decimal)(random.NextDouble() * (double)(mockBill.MaxAmount - mockBill.MinAmount) + (double)mockBill.MinAmount), 2);
            var usage = mockBill.Unit != null ? random.Next(100, 1000) : (int?)null;

            var invoiceNumber = $"SYNC-{currentDate:yyyyMMdd}-{random.Next(10000, 99999)}";

            var attributes = new Dictionary<string, object>
            {
                ["utilityType"] = mockBill.Utility,
                ["utilityProvider"] = mockBill.Vendor,
                ["invoiceNumber"] = invoiceNumber,
                ["syncedAt"] = currentDate.ToString("O"),
                ["billingPeriodStart"] = currentDate.AddMonths(-1).ToString("yyyy-MM-dd"),
                ["billingPeriodEnd"] = currentDate.AddDays(-1).ToString("yyyy-MM-dd"),
            };

            if (usage.HasValue && mockBill.Unit != null)
            {
                attributes["usage"] = usage.Value;
                attributes["usageUnit"] = mockBill.Unit;
            }

            var item = new Item
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExternalId = invoiceNumber,
                Date = DateTime.SpecifyKind(currentDate.AddDays(-random.Next(1, 15)), DateTimeKind.Utc),
                Amount = amount,
                Quantity = usage,
                Description = $"{mockBill.Utility} bill for {property.Name} - Synced {currentDate:MMM dd, yyyy HH:mm}",
                Attributes = JsonSerializer.Serialize(attributes),
                Source = ItemSource.Pipeline,
                BatchId = batchId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Items.Add(item);
            syncedItems.Add(item);

            // Link to property
            _context.ItemEntities.Add(new ItemEntity
            {
                ItemId = item.Id,
                EntityId = property.Id,
                AssignedAt = DateTime.UtcNow
            });

            // Try to link to matching utility type
            var utilityEntity = utilityEntities.FirstOrDefault(e =>
                e.Name.Contains(mockBill.Utility, StringComparison.OrdinalIgnoreCase));
            if (utilityEntity != null)
            {
                _context.ItemEntities.Add(new ItemEntity
                {
                    ItemId = item.Id,
                    EntityId = utilityEntity.Id,
                    AssignedAt = DateTime.UtcNow
                });
            }

            // Try to link to matching vendor
            var vendorEntity = vendorEntities.FirstOrDefault(e =>
                e.Name.Contains(mockBill.Vendor.Split(' ')[0], StringComparison.OrdinalIgnoreCase));
            if (vendorEntity != null)
            {
                _context.ItemEntities.Add(new ItemEntity
                {
                    ItemId = item.Id,
                    EntityId = vendorEntity.Id,
                    AssignedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new MockSyncResponse(
            syncedItems.Count,
            batchId,
            currentDate,
            $"Successfully synced {syncedItems.Count} new items"
        ));
    }
}

public record MockSyncResponse(
    int ItemCount,
    Guid BatchId,
    DateTime SyncedAt,
    string Message
);
