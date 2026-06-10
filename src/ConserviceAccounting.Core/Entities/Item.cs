using ConserviceAccounting.Core.Enums;

namespace ConserviceAccounting.Core.Entities;

public class Item
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? ExternalId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public decimal? Quantity { get; set; }
    public string? Description { get; set; }
    public string? Attributes { get; set; }
    public ItemSource Source { get; set; }
    public Guid? BatchId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;

    // Items are linked to entities via bridge table
    public ICollection<ItemEntity> ItemEntities { get; set; } = new List<ItemEntity>();
}
