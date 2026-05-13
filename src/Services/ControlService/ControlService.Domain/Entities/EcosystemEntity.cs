using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;
using Control.Domain.Events;

namespace Control.Domain.Entities;

public sealed class EcosystemEntity : AggregateRoot, IEntity
{
    private EcosystemEntity(
        Guid id, 
        Guid userId,
        EcosystemTypeEnum type,
        string name, 
        double? volume,
        Guid controllerId, 
        DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        Type = type;
        Name = name;
        Volume = volume;
        ControllerId = controllerId; 
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public EcosystemTypeEnum Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public double? Volume { get; private set; } 
    public Guid ControllerId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<EcosystemEntity> Create(
        Guid userId,
        EcosystemTypeEnum type,
        string name,
        double? volume,
        Guid controllerId)
    {
        var errors = new List<string>();

        if (userId == Guid.Empty)
        {
            errors.Add("userId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("name must not be empty.");
        }

        if (controllerId == Guid.Empty)
        {
            errors.Add("controllerId must not be empty.");
        }

        if (volume <= 0)
        {
            errors.Add("volume must be positive.");
        }

        if (errors.Count > 0)
        {
            return Result<EcosystemEntity>.Failure(
                Error.Validation(
                    "Ecosystem.Invalid",
                    string.Join("; ", errors)));
        }

        var ecosystem = new EcosystemEntity(
            Guid.NewGuid(),
            userId,
            type,
            name.Trim(),
            volume,
            controllerId,
            DateTime.UtcNow);

        ecosystem.RaiseEvent(new EcosystemCreatedDomainEvent
        {
            EcosystemId = ecosystem.Id,
            Name = ecosystem.Name,
            UserId = ecosystem.UserId,
            ControllerId = ecosystem.ControllerId,
        });

        return Result<EcosystemEntity>.Success(ecosystem);
    }

    public Result SetName(string name)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
           errors.Add("name must not be empty.");
        }

        if (errors.Count != 0)
        {
            return Result.Failure(Error.Validation(
                "Ecosystem.Invalid",
                string.Join("; ", errors)));
        }

        Name = name;

        RaiseEvent(new EcosystemUdatedDominEvent
        {
            EcosystemId = Id,
            UserId = UserId,
            Name = Name,
            ControllerId = ControllerId,
            CreatedAt = CreatedAt,
        });

        return Result.Success();
    }

    public Result SetVolume(double? volume)
    {
        var errors = new List<string>();

        if (volume >= 0)
        {
            errors.Add("volume must be positive.");
        }

        if (errors.Count > 0)
        {
            return Result.Failure(Error.Validation(
                "Ecosystem.Invalid",
                string.Join("; ", errors)));
        }

        Volume = volume;

        return Result.Success();
    }
}
