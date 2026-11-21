using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Control.Domain.ValueObjects;

namespace Control.Domain.Entities;

public sealed class Sensor : AggregateRoot, IEntity
{
    private Sensor(
        Guid id,
        Guid controllerId,
        Guid ecosystemId,
        Name name,
        SensorState state,
        SensorType type,
        double lastValue,
        DateTime createdAt)
    {
        Id = id;
        ControllerId = controllerId;
        EcosystemId = ecosystemId;
        Name = name;
        State = state;
        Type = type;
        LastValue = lastValue;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618
    private Sensor() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid EcosystemId { get; private set; }
    public Name Name { get; private set; }
    public SensorState State { get; private set; }
    public SensorType Type { get; private set; }
    public double LastValue { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<Sensor> Create(
        Guid id,
        Guid controllerId,
        Guid ecosystemId,
        string rawName,
        SensorState state,
        SensorType type,
        DateTime createdAt)
    {
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(id, "Sensor Id", errors);
        DomainGuard.CheckNotEmpty(controllerId, nameof(ControllerId), errors);
        DomainGuard.CheckNotEmpty(ecosystemId, nameof(EcosystemId), errors);
        Name? name = DomainGuard.Collect(Name.Create(rawName), errors);
        DomainGuard.CheckEnum(state, nameof(State), errors);
        DomainGuard.CheckEnum(type, nameof(Type), errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<Sensor>(errors);
        }

        var sensor = new Sensor(
            id, controllerId, ecosystemId,
            name!, state, type, lastValue: default,
            createdAt);

        return Result<Sensor>.Success(sensor);
    }

    public void SetState(SensorState state)
    {
        if (State == state)
        {
            return;
        }

        State = state;

        IncrementVersion();
    }

    public void SetLastValue(double value)
    {
        LastValue = value;

        IncrementVersion();
    }

    public void SetType(SensorType type)
    {
        if (Type == type)
        {
            return;
        }

        Type = type;

        IncrementVersion();
    }

    public Result SetName(string rawName)
    {
        Result<Name> nameResult = Name.Create(rawName);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        Name = nameResult.Value;

        IncrementVersion();

        return Result.Success();
    }
}
