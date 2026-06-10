using PropertyViewerAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace PropertyViewerAccounting.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Core entities
    public DbSet<User> Users => Set<User>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<DataPush> DataPushes => Set<DataPush>();
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();

    // Integration entities
    public DbSet<AccountingSoftware> AccountingSoftware => Set<AccountingSoftware>();
    public DbSet<ClientAccountingConnection> ClientAccountingConnections => Set<ClientAccountingConnection>();
    public DbSet<ReportSchedule> ReportSchedules => Set<ReportSchedule>();
    public DbSet<ReportScheduleRun> ReportScheduleRuns => Set<ReportScheduleRun>();

    // Domain entities
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Utility> Utilities => Set<Utility>();
    public DbSet<Resident> Residents => Set<Resident>();

    // Bridge tables - User relationships
    public DbSet<UserProperty> UserProperties => Set<UserProperty>();
    public DbSet<UserBill> UserBills => Set<UserBill>();

    // Bridge tables - Property relationships
    public DbSet<PropertyBill> PropertyBills => Set<PropertyBill>();
    public DbSet<PropertyUtility> PropertyUtilities => Set<PropertyUtility>();
    public DbSet<PropertyProvider> PropertyProviders => Set<PropertyProvider>();
    public DbSet<PropertyResident> PropertyResidents => Set<PropertyResident>();

    // Bridge tables - Provider/Utility relationships
    public DbSet<ProviderUtility> ProviderUtilities => Set<ProviderUtility>();
    public DbSet<BillUtility> BillUtilities => Set<BillUtility>();
    public DbSet<BillProvider> BillProviders => Set<BillProvider>();

    // Bridge tables - Composite relationships
    public DbSet<PropertyUtilityBill> PropertyUtilityBills => Set<PropertyUtilityBill>();

    // Entity model - flexible, non-hierarchical objects linked via bridge tables
    public DbSet<EntityType> EntityTypes => Set<EntityType>();
    public DbSet<Entity> Entities => Set<Entity>();
    public DbSet<EntityLink> EntityLinks => Set<EntityLink>();
    public DbSet<ItemEntity> ItemEntities => Set<ItemEntity>();

    // Legacy scope model - to be removed after migration
    public DbSet<Scope> Scopes => Set<Scope>();
    public DbSet<ScopeType> ScopeTypes => Set<ScopeType>();
    public DbSet<ItemScope> ItemScopes => Set<ItemScope>();
    public DbSet<ScopeLink> ScopeLinks => Set<ScopeLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
