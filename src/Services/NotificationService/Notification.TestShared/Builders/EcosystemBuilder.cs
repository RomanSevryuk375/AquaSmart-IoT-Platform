using Contracts.Results;
using Notification.Domain.Entities;
using Notification.TestShared.Constants;

namespace Notification.TestShared.Builders;

public class EcosystemBuilder
{
    private Guid _id = NotificationTestConstants.EcosystemId;
    private Guid _userId = NotificationTestConstants.UserId;
    private string _name = "Test Ecosystem";

    public EcosystemBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public EcosystemBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public EcosystemBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public Ecosystem Build()
    {
        Result<Ecosystem> result = Ecosystem.Create(_id, _userId, _name);

        if (result.IsFailure)
        {
            throw new ArgumentException($"EcosystemBuilder failed: {result.Error.Message}");
        }

        return result.Value;
    }
}
