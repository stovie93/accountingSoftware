using System.Text.Json;
using PropertyViewerAccounting.Core.Entities;
using PropertyViewerAccounting.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace PropertyViewerAccounting.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Ensure admin user exists (even if database was partially seeded)
        var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.UserEmail == "admin@demo.com");

        if (existingAdmin != null)
        {
            // Reset password to ensure demo credentials work
            existingAdmin.UserPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
            existingAdmin.UserIsActive = true;
            await context.SaveChangesAsync();

            // Ensure report definitions and integrations exist even for existing users
            await EnsureReportDefinitionsExistAsync(context, existingAdmin.UserId);
            await EnsureAccountingIntegrationsExistAsync(context, existingAdmin.UserId);
            return;
        }

        var adminUser = new User
        {
            UserId = Guid.NewGuid(),
            UserName = "sjohnson",
            UserEmail = "admin@demo.com",
            UserPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            UserFirstName = "Sarah",
            UserLastName = "Johnson",
            UserTitle = "Property Manager",
            UserGroup = "Management",
            UserRole = UserRole.Admin,
            UserIsActive = true,
            UserCreatedAt = DateTime.UtcNow,
            UserUpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        var userId = adminUser.UserId;

        // Create entity types (categories for organizing entities)
        var propertyEntityType = CreateEntityType(userId, "Property", "property", "Physical properties being managed", "#3B82F6", 0);
        var utilityTypeEntityType = CreateEntityType(userId, "Utility Type", "utility-type", "Types of utilities (Electric, Water, Gas, etc.)", "#10B981", 1);
        var vendorEntityType = CreateEntityType(userId, "Vendor", "vendor", "Service providers and vendors", "#F59E0B", 2);
        var billEntityType = CreateEntityType(userId, "Bill", "bill", "Bills and invoices", "#8B5CF6", 3);

        context.EntityTypes.AddRange(propertyEntityType, utilityTypeEntityType, vendorEntityType, billEntityType);

        // Create property entities
        var oakwoodEntity = CreateEntity(userId, propertyEntityType.Id, "Oakwood Apartments", "oakwood-apts", "123 Oak Street, Denver, CO 80202", 0);
        var maplegroveEntity = CreateEntity(userId, propertyEntityType.Id, "Maplegrove Complex", "maplegrove", "456 Maple Ave, Denver, CO 80203", 1);
        var pinecrestEntity = CreateEntity(userId, propertyEntityType.Id, "Pinecrest Towers", "pinecrest", "789 Pine Blvd, Denver, CO 80204", 2);
        var cedarviewEntity = CreateEntity(userId, propertyEntityType.Id, "Cedarview Estates", "cedarview", "321 Cedar Lane, Denver, CO 80205", 3);

        // Create utility type entities
        var electricEntity = CreateEntity(userId, utilityTypeEntityType.Id, "Electric", "electric", "Electric utility charges", 0);
        var waterEntity = CreateEntity(userId, utilityTypeEntityType.Id, "Water", "water", "Water utility charges", 1);
        var gasEntity = CreateEntity(userId, utilityTypeEntityType.Id, "Gas", "gas", "Natural gas charges", 2);
        var sewerEntity = CreateEntity(userId, utilityTypeEntityType.Id, "Sewer", "sewer", "Sewer and waste charges", 3);
        var trashEntity = CreateEntity(userId, utilityTypeEntityType.Id, "Trash", "trash", "Trash collection charges", 4);

        // Create vendor entities
        var denverPowerEntity = CreateEntity(userId, vendorEntityType.Id, "Denver Power & Light", "denver-power", "Electric utility provider", 0);
        var denverWaterEntity = CreateEntity(userId, vendorEntityType.Id, "Denver Water Authority", "denver-water", "Water utility provider", 1);
        var coloradoGasEntity = CreateEntity(userId, vendorEntityType.Id, "Colorado Natural Gas", "colorado-gas", "Gas utility provider", 2);
        var denverSewerEntity = CreateEntity(userId, vendorEntityType.Id, "Denver Metro Wastewater", "denver-sewer", "Sewer utility provider", 3);
        var ecoWasteEntity = CreateEntity(userId, vendorEntityType.Id, "EcoWaste Services", "ecowaste", "Trash collection provider", 4);

        var propertyEntities = new[] { oakwoodEntity, maplegroveEntity, pinecrestEntity, cedarviewEntity };
        var utilityEntities = new[] { electricEntity, waterEntity, gasEntity, sewerEntity, trashEntity };
        var vendorEntities = new[] { denverPowerEntity, denverWaterEntity, coloradoGasEntity, denverSewerEntity, ecoWasteEntity };

        context.Entities.AddRange(propertyEntities);
        context.Entities.AddRange(utilityEntities);
        context.Entities.AddRange(vendorEntities);

        await context.SaveChangesAsync();

        // Create entity links: Vendor serves Property
        foreach (var property in propertyEntities)
        {
            foreach (var vendor in vendorEntities)
            {
                context.EntityLinks.Add(new EntityLink
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    SourceEntityId = vendor.Id,
                    TargetEntityId = property.Id,
                    LinkType = "serves",
                    Description = $"{vendor.Name} provides service to {property.Name}",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // Create entity links: Vendor provides UtilityType
        var vendorUtilityMap = new[]
        {
            (Vendor: denverPowerEntity, Utility: electricEntity),
            (Vendor: denverWaterEntity, Utility: waterEntity),
            (Vendor: coloradoGasEntity, Utility: gasEntity),
            (Vendor: denverSewerEntity, Utility: sewerEntity),
            (Vendor: ecoWasteEntity, Utility: trashEntity)
        };

        foreach (var (vendor, utility) in vendorUtilityMap)
        {
            context.EntityLinks.Add(new EntityLink
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SourceEntityId = vendor.Id,
                TargetEntityId = utility.Id,
                LinkType = "provides",
                Description = $"{vendor.Name} provides {utility.Name} service",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Property details for generating bills
        var propertyData = new[]
        {
            new { Entity = oakwoodEntity, Name = "Oakwood Apartments", BaseElectric = 2800m, BaseWater = 1200m, BaseGas = 800m, BaseSewer = 400m, BaseTrash = 350m },
            new { Entity = maplegroveEntity, Name = "Maplegrove Complex", BaseElectric = 4200m, BaseWater = 1800m, BaseGas = 1200m, BaseSewer = 600m, BaseTrash = 525m },
            new { Entity = pinecrestEntity, Name = "Pinecrest Towers", BaseElectric = 7000m, BaseWater = 3000m, BaseGas = 2000m, BaseSewer = 1000m, BaseTrash = 875m },
            new { Entity = cedarviewEntity, Name = "Cedarview Estates", BaseElectric = 2100m, BaseWater = 900m, BaseGas = 600m, BaseSewer = 300m, BaseTrash = 262m }
        };

        // Generate 6 months of utility bills (Aug 2025 - Jan 2026)
        var random = new Random(42);
        var months = new[]
        {
            new DateTime(2025, 8, 1), new DateTime(2025, 9, 1), new DateTime(2025, 10, 1),
            new DateTime(2025, 11, 1), new DateTime(2025, 12, 1), new DateTime(2026, 1, 1)
        };

        var utilityInfo = new[]
        {
            (UtilityEntity: electricEntity, VendorEntity: denverPowerEntity, Name: "Electric", Unit: "kWh", GetBase: (Func<dynamic, decimal>)(p => p.BaseElectric)),
            (UtilityEntity: waterEntity, VendorEntity: denverWaterEntity, Name: "Water", Unit: "gallons", GetBase: (Func<dynamic, decimal>)(p => p.BaseWater)),
            (UtilityEntity: gasEntity, VendorEntity: coloradoGasEntity, Name: "Gas", Unit: "therms", GetBase: (Func<dynamic, decimal>)(p => p.BaseGas)),
            (UtilityEntity: sewerEntity, VendorEntity: denverSewerEntity, Name: "Sewer", Unit: (string?)null, GetBase: (Func<dynamic, decimal>)(p => p.BaseSewer)),
            (UtilityEntity: trashEntity, VendorEntity: ecoWasteEntity, Name: "Trash", Unit: "pickups", GetBase: (Func<dynamic, decimal>)(p => p.BaseTrash))
        };

        var batchId = Guid.NewGuid();

        foreach (var month in months)
        {
            var seasonalElectric = month.Month is 6 or 7 or 8 or 12 or 1 or 2 ? 1.3m : 1.0m;
            var seasonalGas = month.Month is 11 or 12 or 1 or 2 or 3 ? 1.8m : 0.4m;

            foreach (var prop in propertyData)
            {
                foreach (var utility in utilityInfo)
                {
                    var baseAmount = utility.GetBase(prop);
                    var seasonal = utility.Name == "Electric" ? seasonalElectric : (utility.Name == "Gas" ? seasonalGas : 1.0m);
                    var variance = utility.Name == "Trash" ? 1.0m : (0.9m + (decimal)random.NextDouble() * 0.2m);
                    var amount = Math.Round(baseAmount * seasonal * variance, 2);

                    int? usage = utility.Name switch
                    {
                        "Electric" => (int)(amount / 0.12m),
                        "Water" => (int)(amount / 0.004m),
                        "Gas" => (int)(amount / 1.2m),
                        "Trash" => 4,
                        _ => null
                    };

                    var item = CreateUtilityBill(userId, prop.Entity.Code, month, amount, usage, utility.Unit, utility.Name, batchId);
                    context.Items.Add(item);

                    // Link item to multiple entities: Property + Utility Type + Vendor
                    context.ItemEntities.Add(new ItemEntity { ItemId = item.Id, EntityId = prop.Entity.Id, AssignedAt = DateTime.UtcNow });
                    context.ItemEntities.Add(new ItemEntity { ItemId = item.Id, EntityId = utility.UtilityEntity.Id, AssignedAt = DateTime.UtcNow });
                    context.ItemEntities.Add(new ItemEntity { ItemId = item.Id, EntityId = utility.VendorEntity.Id, AssignedAt = DateTime.UtcNow });
                }
            }
        }

        await context.SaveChangesAsync();

        // Seed Domain Entities (new tables)
        await SeedDomainEntitiesAsync(context, userId);

        // Seed Report Definitions
        await SeedReportDefinitionsAsync(context, userId);

        // Seed Accounting Software Integration data
        await SeedAccountingIntegrationsAsync(context, userId);
    }

    private static async Task SeedDomainEntitiesAsync(AppDbContext context, Guid userId)
    {
        // Create Utilities
        var utilities = new[]
        {
            CreateUtility(userId, "Electric", "Electric", "ELEC", "Electricity service"),
            CreateUtility(userId, "Water", "Water", "WTR", "Water service"),
            CreateUtility(userId, "Natural Gas", "Gas", "GAS", "Natural gas service"),
            CreateUtility(userId, "Sewer", "Sewer", "SWR", "Sewer and wastewater service"),
            CreateUtility(userId, "Trash", "Trash", "TRS", "Trash collection service"),
            CreateUtility(userId, "Internet", "Internet", "INT", "Internet service"),
            CreateUtility(userId, "Cable TV", "Cable", "CBL", "Cable television service"),
        };
        context.Utilities.AddRange(utilities);

        // Create Providers
        var providers = new[]
        {
            CreateProvider(userId, "DPL-001", "Denver Power & Light", "DPL", "Electric utility provider", "Electric", "123 Power Way, Denver, CO 80202", "Denver", "CO", "80202", "USA", "(303) 555-0100", "billing@denverpower.com", "https://denverpower.com", "John Smith", "ACC-DPL-12345"),
            CreateProvider(userId, "DWA-001", "Denver Water Authority", "DWA", "Municipal water provider", "Water", "456 Water St, Denver, CO 80203", "Denver", "CO", "80203", "USA", "(303) 555-0200", "service@denverwater.gov", "https://denverwater.gov", "Jane Doe", "ACC-DWA-67890"),
            CreateProvider(userId, "CNG-001", "Colorado Natural Gas", "CNG", "Natural gas provider", "Gas", "789 Gas Blvd, Denver, CO 80204", "Denver", "CO", "80204", "USA", "(303) 555-0300", "support@coloradogas.com", "https://coloradogas.com", "Bob Wilson", "ACC-CNG-11111"),
            CreateProvider(userId, "DMW-001", "Denver Metro Wastewater", "DMW", "Sewer and wastewater services", "Sewer", "321 Waste Ave, Denver, CO 80205", "Denver", "CO", "80205", "USA", "(303) 555-0400", "info@denverwaste.gov", "https://denverwaste.gov", "Sarah Brown", "ACC-DMW-22222"),
            CreateProvider(userId, "EWS-001", "EcoWaste Services", "EWS", "Trash and recycling collection", "Trash", "654 Green Rd, Denver, CO 80206", "Denver", "CO", "80206", "USA", "(303) 555-0500", "hello@ecowaste.com", "https://ecowaste.com", "Mike Green", "ACC-EWS-33333"),
            CreateProvider(userId, "XFI-001", "Xfinity Business", "XFI", "Internet and cable provider", "Telecom", "987 Connect Dr, Denver, CO 80207", "Denver", "CO", "80207", "USA", "(303) 555-0600", "business@xfinity.com", "https://business.xfinity.com", "Lisa Chen", "ACC-XFI-44444"),
        };
        context.Providers.AddRange(providers);

        // Create Properties
        var properties = new[]
        {
            CreateProperty(userId, "OAK-001", "Oakwood Apartments", "123 Oak Street", "Denver", "CO", "80202", "USA", "Residential", "OAK", "24-unit apartment complex in downtown Denver", 24, 18000m),
            CreateProperty(userId, "MAP-001", "Maplegrove Complex", "456 Maple Avenue", "Denver", "CO", "80203", "USA", "Residential", "MAP", "36-unit apartment complex with pool", 36, 27000m),
            CreateProperty(userId, "PIN-001", "Pinecrest Towers", "789 Pine Boulevard", "Denver", "CO", "80204", "USA", "Residential", "PIN", "60-unit high-rise apartment building", 60, 45000m),
            CreateProperty(userId, "CED-001", "Cedarview Estates", "321 Cedar Lane", "Denver", "CO", "80205", "USA", "Residential", "CED", "18-unit townhome community", 18, 13500m),
            CreateProperty(userId, "ELM-001", "Elmwood Plaza", "555 Elm Court", "Denver", "CO", "80206", "USA", "Commercial", "ELM", "10-unit retail strip mall", 10, 25000m),
            CreateProperty(userId, "BIR-001", "Birchwood Office Park", "777 Birch Drive", "Denver", "CO", "80207", "USA", "Commercial", "BIR", "5-building office complex", 5, 75000m),
        };
        context.Properties.AddRange(properties);

        await context.SaveChangesAsync();

        // Create Property-Provider relationships (each property linked to all providers)
        foreach (var property in properties)
        {
            foreach (var provider in providers)
            {
                context.PropertyProviders.Add(new PropertyProvider
                {
                    PropertyProviderId = Guid.NewGuid(),
                    PropertyId = property.PropertyId,
                    ProviderId = provider.ProviderId,
                    PropertyProviderAccountNumber = $"{property.PropertyCode}-{provider.ProviderCode}",
                    PropertyProviderIsPrimary = true,
                    PropertyProviderIsActive = true,
                    PropertyProviderCreatedAt = DateTime.UtcNow,
                    PropertyProviderUpdatedAt = DateTime.UtcNow
                });
            }
        }

        // Create Property-Utility relationships (each property linked to all utility types)
        foreach (var property in properties)
        {
            foreach (var utility in utilities)
            {
                context.PropertyUtilities.Add(new PropertyUtility
                {
                    PropertyUtilityId = Guid.NewGuid(),
                    PropertyId = property.PropertyId,
                    UtilityId = utility.UtilityId,
                    PropertyUtilityAccountNumber = $"{property.PropertyCode}-{utility.UtilityCode}",
                    PropertyUtilityIsActive = true,
                    PropertyUtilityCreatedAt = DateTime.UtcNow,
                    PropertyUtilityUpdatedAt = DateTime.UtcNow
                });
            }
        }

        // Create Provider-Utility relationships (link each provider to its utility type)
        for (int i = 0; i < providers.Length && i < utilities.Length; i++)
        {
            context.ProviderUtilities.Add(new ProviderUtility
            {
                ProviderUtilityId = Guid.NewGuid(),
                ProviderId = providers[i].ProviderId,
                UtilityId = utilities[i].UtilityId,
                ProviderUtilityIsPrimary = true,
                ProviderUtilityIsActive = true,
                ProviderUtilityCreatedAt = DateTime.UtcNow,
                ProviderUtilityUpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();

        // Create Bills - 6 months of utility bills for each property
        var utilityMap = new Dictionary<string, (Utility Utility, Provider Provider)>
        {
            ["Electric"] = (utilities[0], providers[0]),
            ["Water"] = (utilities[1], providers[1]),
            ["Gas"] = (utilities[2], providers[2]),
            ["Sewer"] = (utilities[3], providers[3]),
            ["Trash"] = (utilities[4], providers[4])
        };

        var random = new Random(42);
        var months = new[]
        {
            new DateTime(2025, 8, 1), new DateTime(2025, 9, 1), new DateTime(2025, 10, 1),
            new DateTime(2025, 11, 1), new DateTime(2025, 12, 1), new DateTime(2026, 1, 1)
        };

        var billNumber = 1;
        foreach (var month in months)
        {
            var seasonalElectric = month.Month is 6 or 7 or 8 or 12 or 1 or 2 ? 1.3m : 1.0m;
            var seasonalGas = month.Month is 11 or 12 or 1 or 2 or 3 ? 1.8m : 0.4m;

            foreach (var property in properties)
            {
                var electricAmount = Math.Round(GetBaseAmount(property.PropertyUnitCount ?? 10, 120) * seasonalElectric * GetVariance(random), 2);
                var electricBill = CreateBill(userId, $"BILL-{billNumber++:D6}", electricAmount, "Needs approval", $"Electric bill for {property.PropertyName}", month, month.AddDays(30), $"{month:MMM} 1", $"{month.AddMonths(1).AddDays(-1):MMM d}", (int)(electricAmount / 0.12m), "kWh", 0.12m, "Electric");
                context.Bills.Add(electricBill);
                AddBillRelationships(context, electricBill, property, utilityMap["Electric"]);

                var waterAmount = Math.Round(GetBaseAmount(property.PropertyUnitCount ?? 10, 50) * GetVariance(random), 2);
                var waterBill = CreateBill(userId, $"BILL-{billNumber++:D6}", waterAmount, "Pending", $"Water bill for {property.PropertyName}", month, month.AddDays(30), $"{month:MMM} 1", $"{month.AddMonths(1).AddDays(-1):MMM d}", (int)(waterAmount / 0.004m), "gallons", 0.004m, "Water");
                context.Bills.Add(waterBill);
                AddBillRelationships(context, waterBill, property, utilityMap["Water"]);

                var gasAmount = Math.Round(GetBaseAmount(property.PropertyUnitCount ?? 10, 35) * seasonalGas * GetVariance(random), 2);
                var gasBill = CreateBill(userId, $"BILL-{billNumber++:D6}", gasAmount, "Needs approval", $"Gas bill for {property.PropertyName}", month, month.AddDays(30), $"{month:MMM} 1", $"{month.AddMonths(1).AddDays(-1):MMM d}", (int)(gasAmount / 1.2m), "therms", 1.2m, "Gas");
                context.Bills.Add(gasBill);
                AddBillRelationships(context, gasBill, property, utilityMap["Gas"]);

                var sewerAmount = Math.Round(waterAmount * 0.35m, 2);
                var sewerBill = CreateBill(userId, $"BILL-{billNumber++:D6}", sewerAmount, "Pending", $"Sewer bill for {property.PropertyName}", month, month.AddDays(30), $"{month:MMM} 1", $"{month.AddMonths(1).AddDays(-1):MMM d}", null, null, null, "Sewer");
                context.Bills.Add(sewerBill);
                AddBillRelationships(context, sewerBill, property, utilityMap["Sewer"]);

                var trashAmount = Math.Round((property.PropertyUnitCount ?? 10) * 15m, 2);
                var trashBill = CreateBill(userId, $"BILL-{billNumber++:D6}", trashAmount, "Paid", $"Trash bill for {property.PropertyName}", month, month.AddDays(30), $"{month:MMM} 1", $"{month.AddMonths(1).AddDays(-1):MMM d}", 4, "pickups", trashAmount / 4, "Trash");
                context.Bills.Add(trashBill);
                AddBillRelationships(context, trashBill, property, utilityMap["Trash"]);
            }
        }

        await context.SaveChangesAsync();
    }

    private static void AddBillRelationships(AppDbContext context, Bill bill, Property property, (Utility Utility, Provider Provider) utilityInfo)
    {
        context.PropertyBills.Add(new PropertyBill
        {
            PropertyBillId = Guid.NewGuid(),
            PropertyId = property.PropertyId,
            BillId = bill.BillId,
            PropertyBillAllocationMethod = "Full",
            PropertyBillAllocationPercent = 100m,
            PropertyBillIsPrimary = true,
            PropertyBillIsActive = true,
            PropertyBillCreatedAt = DateTime.UtcNow,
            PropertyBillUpdatedAt = DateTime.UtcNow
        });

        context.BillProviders.Add(new BillProvider
        {
            BillProviderId = Guid.NewGuid(),
            BillId = bill.BillId,
            ProviderId = utilityInfo.Provider.ProviderId,
            BillProviderIsPrimary = true,
            BillProviderIsActive = true,
            BillProviderCreatedAt = DateTime.UtcNow,
            BillProviderUpdatedAt = DateTime.UtcNow
        });

        context.BillUtilities.Add(new BillUtility
        {
            BillUtilityId = Guid.NewGuid(),
            BillId = bill.BillId,
            UtilityId = utilityInfo.Utility.UtilityId,
            BillUtilityAmount = bill.BillAmount,
            BillUtilityQuantity = bill.BillQuantity,
            BillUtilityUnit = bill.BillUnit,
            BillUtilityRate = bill.BillRate,
            BillUtilityIsPrimary = true,
            BillUtilityIsActive = true,
            BillUtilityCreatedAt = DateTime.UtcNow,
            BillUtilityUpdatedAt = DateTime.UtcNow
        });
    }

    private static decimal GetBaseAmount(int units, decimal perUnit) => units * perUnit;
    private static decimal GetVariance(Random random) => 0.9m + (decimal)random.NextDouble() * 0.2m;

    private static Utility CreateUtility(Guid userId, string name, string type, string code, string description)
    {
        return new Utility
        {
            UtilityId = Guid.NewGuid(),
            UserId = userId,
            UtilityName = name,
            UtilityType = type,
            UtilityCode = code,
            UtilityDescription = description,
            UtilityIsActive = true,
            UtilityCreatedAt = DateTime.UtcNow,
            UtilityUpdatedAt = DateTime.UtcNow
        };
    }

    private static Provider CreateProvider(Guid userId, string accountId, string name, string code, string description, string type, string address, string city, string state, string zip, string country, string phone, string email, string website, string contactName, string accountNumber)
    {
        return new Provider
        {
            ProviderId = Guid.NewGuid(),
            UserId = userId,
            ProviderAccountId = accountId,
            ProviderName = name,
            ProviderCode = code,
            ProviderDescription = description,
            ProviderType = type,
            ProviderAddress = address,
            ProviderCity = city,
            ProviderState = state,
            ProviderZipCode = zip,
            ProviderCountry = country,
            ProviderPhone = phone,
            ProviderEmail = email,
            ProviderWebsite = website,
            ProviderContactName = contactName,
            ProviderAccountNumber = accountNumber,
            ProviderIsActive = true,
            ProviderCreatedAt = DateTime.UtcNow,
            ProviderUpdatedAt = DateTime.UtcNow
        };
    }

    private static Property CreateProperty(Guid userId, string clientId, string name, string address, string city, string state, string zip, string country, string type, string code, string description, int unitCount, decimal sqft)
    {
        return new Property
        {
            PropertyId = Guid.NewGuid(),
            UserId = userId,
            PropertyClientId = clientId,
            PropertyName = name,
            PropertyAddress = address,
            PropertyCity = city,
            PropertyState = state,
            PropertyZipCode = zip,
            PropertyCountry = country,
            PropertyType = type,
            PropertyCode = code,
            PropertyDescription = description,
            PropertyUnitCount = unitCount,
            PropertySquareFootage = sqft,
            PropertyIsActive = true,
            PropertyCreatedAt = DateTime.UtcNow,
            PropertyUpdatedAt = DateTime.UtcNow
        };
    }

    private static Bill CreateBill(Guid userId, string refId, decimal amount, string status, string description, DateTime billDate, DateTime dueDate, string periodStart, string periodEnd, int? quantity, string? unit, decimal? rate, string? category = null)
    {
        return new Bill
        {
            BillId = Guid.NewGuid(),
            UserId = userId,
            BillReferenceId = refId,
            BillAmount = amount,
            BillStatus = status,
            BillDescription = description,
            BillDate = DateTime.SpecifyKind(billDate, DateTimeKind.Utc),
            BillDueDate = DateTime.SpecifyKind(dueDate, DateTimeKind.Utc),
            BillPaidDate = status == "Paid" ? DateTime.SpecifyKind(dueDate.AddDays(-5), DateTimeKind.Utc) : null,
            BillPeriodStart = periodStart,
            BillPeriodEnd = periodEnd,
            BillQuantity = quantity,
            BillUnit = unit,
            BillRate = rate,
            BillTotalAmount = amount,
            BillCurrency = "USD",
            BillCategory = category,
            BillIsActive = true,
            BillCreatedAt = DateTime.UtcNow,
            BillUpdatedAt = DateTime.UtcNow
        };
    }

    private static EntityType CreateEntityType(Guid userId, string name, string code, string description, string color, int sortOrder)
    {
        return new EntityType
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Code = code,
            Description = description,
            Color = color,
            SortOrder = sortOrder,
            IsSystem = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Entity CreateEntity(Guid userId, Guid entityTypeId, string name, string code, string description, int sortOrder)
    {
        return new Entity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EntityTypeId = entityTypeId,
            Name = name,
            Code = code,
            Description = description,
            SortOrder = sortOrder,
            IsActive = true,
            IsSystem = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Item CreateUtilityBill(Guid userId, string propertyCode, DateTime month, decimal amount, int? usage, string? unit, string utilityType, Guid batchId)
    {
        var billingPeriodEnd = month.AddMonths(1).AddDays(-1);
        var invoiceNumber = $"INV-{propertyCode.ToUpper()}-{utilityType.ToUpper()[..3]}-{month:yyyyMM}";

        var attributes = new Dictionary<string, object>
        {
            ["propertyCode"] = propertyCode,
            ["utilityType"] = utilityType,
            ["billingPeriodStart"] = month.ToString("yyyy-MM-dd"),
            ["billingPeriodEnd"] = billingPeriodEnd.ToString("yyyy-MM-dd"),
            ["invoiceNumber"] = invoiceNumber,
            ["utilityProvider"] = utilityType switch
            {
                "Electric" => "Denver Power & Light",
                "Water" => "Denver Water Authority",
                "Gas" => "Colorado Natural Gas",
                "Sewer" => "Denver Metro Wastewater",
                "Trash" => "EcoWaste Services",
                _ => "Unknown Provider"
            },
            ["accountNumber"] = $"{propertyCode.ToUpper()}-{utilityType[..3].ToUpper()}-{new Random(propertyCode.GetHashCode()).Next(10000, 99999)}"
        };

        if (usage.HasValue && unit != null)
        {
            attributes["usage"] = usage.Value;
            attributes["usageUnit"] = unit;
            attributes["ratePerUnit"] = Math.Round(amount / usage.Value, 4);
        }

        return new Item
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExternalId = invoiceNumber,
            Date = DateTime.SpecifyKind(month, DateTimeKind.Utc),
            Amount = Math.Round(amount, 2),
            Quantity = usage,
            Description = $"{utilityType} bill for {propertyCode} - {month:MMMM yyyy}",
            Attributes = JsonSerializer.Serialize(attributes),
            Source = ItemSource.Pipeline,
            BatchId = batchId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static async Task EnsureReportDefinitionsExistAsync(AppDbContext context, Guid userId)
    {
        if (!await context.ReportDefinitions.AnyAsync(r => r.UserId == userId))
        {
            await SeedReportDefinitionsAsync(context, userId);
        }
    }

    private static async Task EnsureAccountingIntegrationsExistAsync(AppDbContext context, Guid userId)
    {
        if (!await context.AccountingSoftware.AnyAsync())
        {
            await SeedAccountingIntegrationsAsync(context, userId);
        }
        else
        {
            // Refresh schedules to pick up any seeder changes
            var existingRuns = context.ReportScheduleRuns.Where(r => r.Schedule.UserId == userId);
            context.ReportScheduleRuns.RemoveRange(existingRuns);
            var existingSchedules = context.ReportSchedules.Where(s => s.UserId == userId);
            context.ReportSchedules.RemoveRange(existingSchedules);
            await context.SaveChangesAsync();

            // Re-seed schedules with existing connections and reports
            await ReseedSchedulesAsync(context, userId);
        }
    }

    private static async Task ReseedSchedulesAsync(AppDbContext context, Guid userId)
    {
        var connections = await context.ClientAccountingConnections.Where(c => c.UserId == userId).ToListAsync();
        var reportDefinitions = await context.ReportDefinitions.Where(r => r.UserId == userId).Take(3).ToListAsync();

        if (connections.Count < 4 || reportDefinitions.Count < 3) return;

        var schedules = new List<ReportSchedule>();

        var dailyTime = new TimeOnly(6, 0);
        var nextRunLocal = DateTime.Today.AddDays(1).Add(dailyTime.ToTimeSpan());
        schedules.Add(new ReportSchedule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ReportDefinitionId = reportDefinitions[0].Id,
            ConnectionId = connections[0].Id,
            Name = "Daily Utility Push to Yardi",
            Description = "Automatically push utility bill data to Yardi every day at 6 AM",
            Frequency = ScheduleFrequency.Daily,
            TimeOfDay = dailyTime,
            ExportFormat = "csv",
            DestinationTable = "UtilityCharges",
            IsActive = true,
            NextRunAt = nextRunLocal.ToUniversalTime(),
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var weeklyTime = new TimeOnly(8, 0);
        var weeklyNextRunLocal = GetNextWeekday(System.DayOfWeek.Monday).Add(weeklyTime.ToTimeSpan());
        schedules.Add(new ReportSchedule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ReportDefinitionId = reportDefinitions[1].Id,
            ConnectionId = connections[1].Id,
            Name = "Weekly Summary to Entrata",
            Description = "Push weekly utility summary to Entrata every Monday at 8 AM",
            Frequency = ScheduleFrequency.Weekly,
            TimeOfDay = weeklyTime,
            DayOfWeek = (int)System.DayOfWeek.Monday,
            ExportFormat = "json",
            IsActive = true,
            NextRunAt = weeklyNextRunLocal.ToUniversalTime(),
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var monthlyTime = new TimeOnly(6, 0);
        var monthlyNextRunLocal = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).Add(monthlyTime.ToTimeSpan());
        schedules.Add(new ReportSchedule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ReportDefinitionId = reportDefinitions[2].Id,
            ConnectionId = connections[3].Id,
            Name = "Monthly Export to MRI",
            Description = "Export monthly billing data to MRI via SFTP on the 1st of each month at 6 AM",
            Frequency = ScheduleFrequency.Monthly,
            TimeOfDay = monthlyTime,
            DayOfMonth = 1,
            ExportFormat = "csv",
            DestinationPath = "/imports/sunrise/monthly",
            IsActive = true,
            NextRunAt = monthlyNextRunLocal.ToUniversalTime(),
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        context.ReportSchedules.AddRange(schedules);
        await context.SaveChangesAsync();

        // Add some run history for the daily schedule
        var dailySchedule = schedules[0];
        var runs = new[]
        {
            new ReportScheduleRun
            {
                Id = Guid.NewGuid(),
                ScheduleId = dailySchedule.Id,
                Status = ScheduleRunStatus.Success,
                StartedAt = DateTime.UtcNow.AddDays(-3).AddHours(6),
                CompletedAt = DateTime.UtcNow.AddDays(-3).AddHours(6).AddMinutes(2),
                RecordsProcessed = 145,
                RecordsFailed = 0,
                OutputFilePath = "/exports/utility_bills_20260118.csv",
                FileSizeBytes = 45230
            },
            new ReportScheduleRun
            {
                Id = Guid.NewGuid(),
                ScheduleId = dailySchedule.Id,
                Status = ScheduleRunStatus.Success,
                StartedAt = DateTime.UtcNow.AddDays(-2).AddHours(6),
                CompletedAt = DateTime.UtcNow.AddDays(-2).AddHours(6).AddMinutes(3),
                RecordsProcessed = 152,
                RecordsFailed = 2,
                ErrorMessage = "2 records skipped due to missing property codes",
                OutputFilePath = "/exports/utility_bills_20260119.csv",
                FileSizeBytes = 47890
            },
            new ReportScheduleRun
            {
                Id = Guid.NewGuid(),
                ScheduleId = dailySchedule.Id,
                Status = ScheduleRunStatus.Success,
                StartedAt = DateTime.UtcNow.AddDays(-1).AddHours(6),
                CompletedAt = DateTime.UtcNow.AddDays(-1).AddHours(6).AddMinutes(2),
                RecordsProcessed = 148,
                RecordsFailed = 0,
                OutputFilePath = "/exports/utility_bills_20260120.csv",
                FileSizeBytes = 46120
            }
        };

        context.ReportScheduleRuns.AddRange(runs);
        dailySchedule.LastRunAt = runs[2].CompletedAt;
        dailySchedule.LastRunStatus = "Success";

        await context.SaveChangesAsync();
    }

    private static async Task SeedReportDefinitionsAsync(AppDbContext context, Guid userId)
    {
        var reportDefinitions = new[]
        {
            new ReportDefinition
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Utility Bills Export",
                Type = ReportType.Export,
                Configuration = JsonSerializer.Serialize(new
                {
                    columns = new[] { "date", "property", "utility", "amount", "vendor" },
                    filters = new { dateRange = "last30days", utilityTypes = new[] { "Electric", "Water", "Gas" } },
                    format = "csv"
                }),
                CreatedById = userId,
                IsShared = true,
                CreatedAt = DateTime.UtcNow
            },
            new ReportDefinition
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Monthly Utility Summary",
                Type = ReportType.Summary,
                Configuration = JsonSerializer.Serialize(new
                {
                    groupBy = "property",
                    aggregations = new[] { "sum", "avg" },
                    period = "monthly",
                    includeCharts = true
                }),
                CreatedById = userId,
                IsShared = true,
                CreatedAt = DateTime.UtcNow
            },
            new ReportDefinition
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Utility Cost Trends",
                Type = ReportType.Trend,
                Configuration = JsonSerializer.Serialize(new
                {
                    metrics = new[] { "totalCost", "usagePerUnit" },
                    timeRange = "6months",
                    compareYearOverYear = true,
                    breakdownBy = "utilityType"
                }),
                CreatedById = userId,
                IsShared = false,
                CreatedAt = DateTime.UtcNow
            },
            new ReportDefinition
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Property Detail Report",
                Type = ReportType.Detail,
                Configuration = JsonSerializer.Serialize(new
                {
                    includeAllBills = true,
                    includeVendorInfo = true,
                    includeUtilityBreakdown = true,
                    sortBy = "date",
                    sortOrder = "desc"
                }),
                CreatedById = userId,
                IsShared = true,
                CreatedAt = DateTime.UtcNow
            },
            new ReportDefinition
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Vendor Payment Export",
                Type = ReportType.Export,
                Configuration = JsonSerializer.Serialize(new
                {
                    columns = new[] { "vendor", "invoiceNumber", "amount", "dueDate", "status" },
                    filters = new { status = new[] { "Pending", "Needs approval" } },
                    format = "xlsx"
                }),
                CreatedById = userId,
                IsShared = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.ReportDefinitions.AddRange(reportDefinitions);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAccountingIntegrationsAsync(AppDbContext context, Guid userId)
    {
        var accountingSoftware = new[]
        {
            new AccountingSoftware
            {
                Id = Guid.NewGuid(),
                Name = "Yardi Voyager",
                Code = "yardi",
                Description = "Enterprise property management and accounting software",
                ConnectionType = "Database",
                LogoUrl = "/logos/yardi.png",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new AccountingSoftware
            {
                Id = Guid.NewGuid(),
                Name = "Entrata",
                Code = "entrata",
                Description = "Cloud-based property management platform",
                ConnectionType = "API",
                LogoUrl = "/logos/entrata.png",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new AccountingSoftware
            {
                Id = Guid.NewGuid(),
                Name = "AppFolio",
                Code = "appfolio",
                Description = "Property management software for residential and commercial",
                ConnectionType = "API",
                LogoUrl = "/logos/appfolio.png",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new AccountingSoftware
            {
                Id = Guid.NewGuid(),
                Name = "RealPage",
                Code = "realpage",
                Description = "Real estate software and analytics platform",
                ConnectionType = "Database",
                LogoUrl = "/logos/realpage.png",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new AccountingSoftware
            {
                Id = Guid.NewGuid(),
                Name = "MRI Software",
                Code = "mri",
                Description = "Real estate software solutions for commercial and residential",
                ConnectionType = "SFTP",
                LogoUrl = "/logos/mri.png",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new AccountingSoftware
            {
                Id = Guid.NewGuid(),
                Name = "ResMan",
                Code = "resman",
                Description = "Property management software for multifamily properties",
                ConnectionType = "API",
                LogoUrl = "/logos/resman.png",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.AccountingSoftware.AddRange(accountingSoftware);
        await context.SaveChangesAsync();

        var yardi = accountingSoftware[0];
        var entrata = accountingSoftware[1];
        var appfolio = accountingSoftware[2];
        var mri = accountingSoftware[4];

        var connections = new[]
        {
            new ClientAccountingConnection
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountingSoftwareId = yardi.Id,
                Name = "Sunrise Yardi Production",
                ConnectionString = "Server=yardi-prod.sunrise.local;Database=YardiVoyager;",
                DatabaseName = "YardiVoyager",
                DatabaseServer = "yardi-prod.sunrise.local",
                DatabaseUsername = "sunrise_integration",
                DatabasePassword = "***encrypted***",
                IsActive = true,
                LastTestedAt = DateTime.UtcNow.AddDays(-1),
                LastTestStatus = "Success",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ClientAccountingConnection
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountingSoftwareId = entrata.Id,
                Name = "Sunrise Entrata API",
                ApiEndpoint = "https://api.entrata.com/v1",
                ApiKey = "pk_sunrise_ent_****",
                ApiSecret = "***encrypted***",
                IsActive = true,
                LastTestedAt = DateTime.UtcNow.AddHours(-6),
                LastTestStatus = "Success",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ClientAccountingConnection
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountingSoftwareId = appfolio.Id,
                Name = "Sunrise AppFolio Integration",
                ApiEndpoint = "https://sunrise.appfolio.com/api/v1",
                ApiKey = "af_key_sunrise_****",
                ApiSecret = "***encrypted***",
                IsActive = true,
                LastTestedAt = DateTime.UtcNow.AddDays(-3),
                LastTestStatus = "Success",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ClientAccountingConnection
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountingSoftwareId = mri.Id,
                Name = "Sunrise MRI SFTP",
                SftpHost = "sftp.mrisoftware.com",
                SftpPort = 22,
                SftpUsername = "sunrise_upload",
                SftpPassword = "***encrypted***",
                SftpPath = "/imports/sunrise/utility-bills",
                IsActive = true,
                LastTestedAt = DateTime.UtcNow.AddDays(-7),
                LastTestStatus = "Success",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ClientAccountingConnection
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountingSoftwareId = yardi.Id,
                Name = "Sunrise Yardi Test Environment",
                ConnectionString = "Server=yardi-test.sunrise.local;Database=YardiVoyager_Test;",
                DatabaseName = "YardiVoyager_Test",
                DatabaseServer = "yardi-test.sunrise.local",
                DatabaseUsername = "sunrise_test",
                DatabasePassword = "***encrypted***",
                IsActive = false,
                LastTestedAt = DateTime.UtcNow.AddDays(-30),
                LastTestStatus = "Failed",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.ClientAccountingConnections.AddRange(connections);
        await context.SaveChangesAsync();

        var reportDefinitions = await context.ReportDefinitions.Take(3).ToListAsync();

        if (reportDefinitions.Count > 0)
        {
            var schedules = new List<ReportSchedule>();

            if (reportDefinitions.Count >= 1)
            {
                var dailyTime = new TimeOnly(6, 0);
                // Calculate NextRunAt using local time so display is consistent with TimeOfDay
                var nextRunLocal = DateTime.Today.AddDays(1).Add(dailyTime.ToTimeSpan());
                schedules.Add(new ReportSchedule
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ReportDefinitionId = reportDefinitions[0].Id,
                    ConnectionId = connections[0].Id,
                    Name = "Daily Utility Push to Yardi",
                    Description = "Automatically push utility bill data to Yardi every day at 6 AM",
                    Frequency = ScheduleFrequency.Daily,
                    TimeOfDay = dailyTime,
                    ExportFormat = "csv",
                    DestinationTable = "UtilityCharges",
                    IsActive = true,
                    NextRunAt = nextRunLocal.ToUniversalTime(),
                    CreatedById = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (reportDefinitions.Count >= 2)
            {
                var weeklyTime = new TimeOnly(8, 0);
                // Calculate NextRunAt using local time so display is consistent with TimeOfDay
                var nextRunLocal = GetNextWeekday(System.DayOfWeek.Monday).Add(weeklyTime.ToTimeSpan());
                schedules.Add(new ReportSchedule
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ReportDefinitionId = reportDefinitions[1].Id,
                    ConnectionId = connections[1].Id,
                    Name = "Weekly Summary to Entrata",
                    Description = "Push weekly utility summary to Entrata every Monday at 8 AM",
                    Frequency = ScheduleFrequency.Weekly,
                    TimeOfDay = weeklyTime,
                    DayOfWeek = (int)System.DayOfWeek.Monday,
                    ExportFormat = "json",
                    IsActive = true,
                    NextRunAt = nextRunLocal.ToUniversalTime(),
                    CreatedById = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (reportDefinitions.Count >= 3)
            {
                var monthlyTime = new TimeOnly(6, 0);
                // Calculate NextRunAt using local time so display is consistent with TimeOfDay
                var nextRunLocal = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).Add(monthlyTime.ToTimeSpan());
                schedules.Add(new ReportSchedule
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ReportDefinitionId = reportDefinitions[2].Id,
                    ConnectionId = connections[3].Id,
                    Name = "Monthly Export to MRI",
                    Description = "Export monthly billing data to MRI via SFTP on the 1st of each month at 6 AM",
                    Frequency = ScheduleFrequency.Monthly,
                    TimeOfDay = monthlyTime,
                    DayOfMonth = 1,
                    ExportFormat = "csv",
                    DestinationPath = "/imports/sunrise/monthly",
                    IsActive = true,
                    NextRunAt = nextRunLocal.ToUniversalTime(),
                    CreatedById = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            context.ReportSchedules.AddRange(schedules);
            await context.SaveChangesAsync();

            if (schedules.Count > 0)
            {
                var dailySchedule = schedules[0];
                var runs = new[]
                {
                    new ReportScheduleRun
                    {
                        Id = Guid.NewGuid(),
                        ScheduleId = dailySchedule.Id,
                        Status = ScheduleRunStatus.Success,
                        StartedAt = DateTime.UtcNow.AddDays(-3).AddHours(6),
                        CompletedAt = DateTime.UtcNow.AddDays(-3).AddHours(6).AddMinutes(2),
                        RecordsProcessed = 145,
                        RecordsFailed = 0,
                        OutputFilePath = "/exports/utility_bills_20260118.csv",
                        FileSizeBytes = 45230
                    },
                    new ReportScheduleRun
                    {
                        Id = Guid.NewGuid(),
                        ScheduleId = dailySchedule.Id,
                        Status = ScheduleRunStatus.Success,
                        StartedAt = DateTime.UtcNow.AddDays(-2).AddHours(6),
                        CompletedAt = DateTime.UtcNow.AddDays(-2).AddHours(6).AddMinutes(3),
                        RecordsProcessed = 152,
                        RecordsFailed = 2,
                        ErrorMessage = "2 records skipped due to missing property codes",
                        OutputFilePath = "/exports/utility_bills_20260119.csv",
                        FileSizeBytes = 47890
                    },
                    new ReportScheduleRun
                    {
                        Id = Guid.NewGuid(),
                        ScheduleId = dailySchedule.Id,
                        Status = ScheduleRunStatus.Success,
                        StartedAt = DateTime.UtcNow.AddDays(-1).AddHours(6),
                        CompletedAt = DateTime.UtcNow.AddDays(-1).AddHours(6).AddMinutes(2),
                        RecordsProcessed = 148,
                        RecordsFailed = 0,
                        OutputFilePath = "/exports/utility_bills_20260120.csv",
                        FileSizeBytes = 46120
                    }
                };

                context.ReportScheduleRuns.AddRange(runs);
                dailySchedule.LastRunAt = runs[2].CompletedAt;
                dailySchedule.LastRunStatus = "Success";

                await context.SaveChangesAsync();
            }
        }
    }

    private static DateTime GetNextWeekday(DayOfWeek dayOfWeek)
    {
        var today = DateTime.Today; // Use local time for consistency with TimeOfDay display
        var daysUntil = ((int)dayOfWeek - (int)today.DayOfWeek + 7) % 7;
        if (daysUntil == 0) daysUntil = 7;
        return today.AddDays(daysUntil);
    }
}
