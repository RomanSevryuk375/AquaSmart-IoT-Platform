using Contracts.Abstractions;

namespace Notification.Domain.Entities;

public sealed class MaintenanceLogEntity : IEntity
{
    private MaintenanceLogEntity(
        Guid id, 
        Guid userId, 
        Guid ecosystemId, 
        DateTime actionDate, 
        Dictionary<string, double> metrics,
        string notes, 
        DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        EcosystemId = ecosystemId;
        ActionDate = actionDate;
        Metrics = metrics;
        Notes = notes;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid EcosystemId { get; private set; }
    public DateTime ActionDate { get; private set; } 
    public Dictionary<string, double> Metrics { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public static (MaintenanceLogEntity? log, List<string> errors) Create(
        Guid userId, 
        Guid aquariumId, 
        DateTime actionDate, 
        Dictionary<string, double>? metrics,
        string notes)
    {
        var errors = new List<string>();
        

        if (errors.Count > 0)
        {
            return (null, errors);
        }

        if (actionDate > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Action date cannot be in the future.");
        }

        var log = new MaintenanceLogEntity(
            Guid.NewGuid(), 
            userId, 
            aquariumId, 
            actionDate, 
            metrics ?? [],
            notes.Trim(), 
            DateTime.UtcNow);

        return (log, errors);
    }
}