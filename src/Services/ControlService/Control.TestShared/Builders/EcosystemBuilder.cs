using Control.TestShared.Constants;

namespace Control.TestShared.Builders;

public class EcosystemBuilder
{
    private Guid _id = ControlTestConstants.EcosystemId;
    private Guid _userId = ControlTestConstants.UserId;
    private EcosystemType _type = EcosystemType.Aquarium;
    private string _name = "Test Ecosystem";
    private double? _volume = 100.0;
    private Guid _controllerId = ControlTestConstants.ControllerId;

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

    public EcosystemBuilder WithType(EcosystemType type)
    {
        _type = type;
        return this;
    }

    public EcosystemBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public EcosystemBuilder WithVolume(double? volume)
    {
        _volume = volume;
        return this;
    }

    public EcosystemBuilder WithControllerId(Guid controllerId)
    {
        _controllerId = controllerId;
        return this;
    }

    public Ecosystem Build()
    {
        Result<Ecosystem> result = Ecosystem.Create(
            _id,
            _userId,
            _type,
            _name,
            _volume,
            _controllerId);

        if (result.IsFailure)
        {
            throw new ArgumentException($"EcosystemBuilder failed: {result.Error.Message}");
        }

        Ecosystem ecosystem = result.Value;
        ecosystem.ClearDomainEvents();
        return ecosystem;
    }
}
