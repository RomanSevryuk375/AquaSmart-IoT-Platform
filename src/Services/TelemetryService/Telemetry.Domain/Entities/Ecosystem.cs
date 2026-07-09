using Contracts.Abstractions;
using Contracts.Results;

namespace Telemetry.Domain.Entities;

public sealed class Ecosystem : AggregateRoot, IEntity
{
    private Ecosystem(
        Guid id,
        Guid controllerId,
        Guid userId,
        DateTime createdAt)
    {
        Id = id;
        ControllerId = controllerId;
        UserId = userId;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618 
    private Ecosystem() { }
#pragma warning restore CS8618 

    public Guid Id { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<Ecosystem> Create(
        Guid ecosystemId,
        Guid controllerId,
        Guid userId)
    {
        var ecosystem = new Ecosystem(
            ecosystemId, controllerId, userId,
            createdAt: DateTime.UtcNow);

        return Result<Ecosystem>.Success(ecosystem);
    }
}
