using Contracts.Abstractions;
using Contracts.Results;

namespace Notification.Domain.Entities;

public sealed class MaintenanceLog : AggregateRoot, IEntity
{
    private MaintenanceLog(
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

#pragma warning disable CS8618
    private MaintenanceLog() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid EcosystemId { get; private set; }
    public DateTime ActionDate { get; private set; }
    public Dictionary<string, double> Metrics { get; private set; }
    public string Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<MaintenanceLog> Create(
        Guid logId,
        Guid userId,
        Guid ecosystemId,
        DateTime actionDate,
        Dictionary<string, double>? metrics,
        string notes)
    {
        if (actionDate > DateTime.UtcNow.AddMinutes(5))
        {
            return Result<MaintenanceLog>.Failure(Error.Validation<MaintenanceLog>(
                "Action date cannot be in the future."));
        }

        var log = new MaintenanceLog(
            logId, userId, ecosystemId,
            actionDate, metrics ?? [], notes.Trim(),
            createdAt: DateTime.UtcNow);

        return Result<MaintenanceLog>.Success(log);
    }
}
