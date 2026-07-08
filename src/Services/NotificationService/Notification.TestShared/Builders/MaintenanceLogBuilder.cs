using Contracts.Results;
using Notification.Domain.Entities;
using Notification.TestShared.Constants;

namespace Notification.TestShared.Builders;

public class MaintenanceLogBuilder
{
    private Guid _id = NotificationTestConstants.MaintenanceLogId;
    private Guid _userId = NotificationTestConstants.UserId;
    private Guid _ecosystemId = NotificationTestConstants.EcosystemId;
    private DateTime _actionDate = DateTime.UtcNow;
    private Dictionary<string, double>? _metrics;
    private string _notes = "Standard maintenance notes";

    public MaintenanceLogBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public MaintenanceLogBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public MaintenanceLogBuilder WithEcosystemId(Guid ecosystemId)
    {
        _ecosystemId = ecosystemId;
        return this;
    }

    public MaintenanceLogBuilder WithActionDate(DateTime actionDate)
    {
        _actionDate = actionDate;
        return this;
    }

    public MaintenanceLogBuilder WithMetrics(Dictionary<string, double>? metrics)
    {
        _metrics = metrics;
        return this;
    }

    public MaintenanceLogBuilder WithNotes(string notes)
    {
        _notes = notes;
        return this;
    }

    public MaintenanceLog Build()
    {
        Result<MaintenanceLog> result = MaintenanceLog.Create(
            _id,
            _userId,
            _ecosystemId,
            _actionDate,
            _metrics,
            _notes);

        if (result.IsFailure)
        {
            throw new ArgumentException($"MaintenanceLogBuilder failed: {result.Error.Message}");
        }

        return result.Value;
    }
}
