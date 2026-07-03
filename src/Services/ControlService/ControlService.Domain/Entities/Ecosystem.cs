using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;
using Control.Domain.Events;
using Control.Domain.ValueObjects;

namespace Control.Domain.Entities;

public sealed class Ecosystem : AggregateRoot, IEntity
{
    private Ecosystem(
        Guid id,
        Guid userId,
        Guid controllerId,
        EcosystemType type,
        Name name,
        Volume? volume,
        DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        ControllerId = controllerId;
        Type = type;
        Name = name;
        Volume = volume;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618
    private Ecosystem() { }
#pragma warning restore CS8618
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public EcosystemType Type { get; private set; }
    public Name Name { get; private set; }
    public Volume? Volume { get; private set; }
    public Guid ControllerId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<Ecosystem> Create(
        Guid ecosystemId,
        Guid userId,
        EcosystemType type,
        string rawName,
        double? rawVolume,
        Guid controllerId)
    {
        var errors = new List<Error>();

        Result<Name> nameResult = Name.Create(rawName);
        if (nameResult.IsFailure)
        {
            errors.Add(nameResult.Error);
        }

        Volume? validVolume = null;
        if (rawVolume.HasValue)
        {
            Result<Volume> volumeResult = Volume.Create(rawVolume.Value);
            if (volumeResult.IsFailure)
            {
                errors.Add(volumeResult.Error);
            }
            validVolume = volumeResult.Value;
        }

        if (errors.Count > 0)
        {
            return Result<Ecosystem>.Failure(Error.Validation<Ecosystem>(
                    string.Join(", ", errors)));
        }

        var ecosystem = new Ecosystem(
            ecosystemId, userId, controllerId,
            type, nameResult.Value, validVolume,
            createdAt: DateTime.UtcNow);

        ecosystem.RaiseEvent(new EcosystemCreatedDomainEvent
        {
            EcosystemId = ecosystem.Id,
            Name = ecosystem.Name.Value,
            UserId = ecosystem.UserId,
            ControllerId = ecosystem.ControllerId,
        });

        return Result<Ecosystem>.Success(ecosystem);
    }

    public Result SetName(string rawName)
    {
        Result<Name> nameResult = Name.Create(rawName);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        Name = nameResult.Value;

        RaiseEvent(new EcosystemUpdatedDomainEvent
        {
            EcosystemId = Id,
            UserId = UserId,
            Name = Name.Value,
            ControllerId = ControllerId,
            CreatedAt = CreatedAt,
        });

        return Result.Success();
    }

    public Result SetVolume(double? rawVolume)
    {
        Volume? validVolume = null;
        if (rawVolume.HasValue)
        {
            Result<Volume> volumeResult = Volume.Create(rawVolume.Value);
            if (volumeResult.IsFailure)
            {
                return Result.Failure(volumeResult.Error);
            }
            validVolume = volumeResult.Value;
        }

        Volume = validVolume;

        return Result.Success();
    }
}
