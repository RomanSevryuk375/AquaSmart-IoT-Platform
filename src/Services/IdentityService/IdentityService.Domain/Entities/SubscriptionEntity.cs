using Contracts.Abstractions;

namespace IdentityService.Domain.Entities;

public class SubscriptionEntity : IEntity
{
    private SubscriptionEntity() { }
    private SubscriptionEntity(
        Guid id, 
        string name, 
        decimal price, 
        int durationDays, 
        List<string> pernissions)
    {
        Id = id;
        Name = name;
        Price = price;
        DurationDays = durationDays;
        Permissions = pernissions;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public int DurationDays { get; private set; }
    public List<string> Permissions { get; private set; } = [];
    public DateTime CreatedAt { get; private set; }

    public static SubscriptionEntity Create(
        Guid id, 
        string name, 
        decimal price, 
        int durationDays,
        List<string> pernissions)
    {
        return new SubscriptionEntity(
            id, 
            name, 
            price, 
            durationDays, 
            pernissions);
    }
}
