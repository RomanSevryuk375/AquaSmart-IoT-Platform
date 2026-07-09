namespace Telemetry.TestShared.Builders;

public class EcosystemBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _controllerId = Guid.NewGuid();
    private Guid _userId = Guid.NewGuid();

    public EcosystemBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public EcosystemBuilder WithControllerId(Guid controllerId)
    {
        _controllerId = controllerId;
        return this;
    }

    public EcosystemBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public Ecosystem Build()
    {
        Result<Ecosystem> result = Ecosystem.Create(_id, _controllerId, _userId);

        if (result.IsFailure)
        {
            throw new ArgumentException($"EcosystemBuilder failed: {result.Error.Message}");
        }

        Ecosystem ecosystem = result.Value;
        ecosystem.ClearDomainEvents();
        return ecosystem;
    }
}
