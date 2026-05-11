using Contracts.Abstractions;
using Contracts.Results;

namespace Telemetry.Domain.Entities;

public sealed class EcosystemEntity : IEntity
{
    private EcosystemEntity(
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

    public Guid Id { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<EcosystemEntity> Create(
        Guid ecosystemId,
        Guid controllerId,
        Guid userId)
    {
        var errors = new List<string>();

        if (ecosystemId == Guid.Empty)
        {
            errors.Add("Ecosystem id must not be empty.");
        }

        if (controllerId == Guid.Empty)
        {
            errors.Add("Controller id must not be empty.");
        }

        if (userId == Guid.Empty)
        {
            errors.Add("User id must not be empty.");
        }

        if (errors.Count > 0)
        {
            return Result<EcosystemEntity>.Failure(
                Error.Validation(
                    "Ecosystem.Invalid", 
                    string.Join("; ", errors)));
        }

        var ecosystem = new EcosystemEntity(
            ecosystemId,
            controllerId,
            userId,
            DateTime.UtcNow);

        return Result<EcosystemEntity>.Success(ecosystem);
    }
}
