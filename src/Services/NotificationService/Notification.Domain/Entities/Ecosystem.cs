using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Results;
using Notification.Domain.ValueObjects;

namespace Notification.Domain.Entities;

public sealed class Ecosystem : AggregateRoot, IEntity
{
    private Ecosystem(
        Guid id,
        Guid userId,
        Name ecosystemName,
        DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        EcosystemName = ecosystemName;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618 
    private Ecosystem() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Name EcosystemName { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<Ecosystem> Create(
        Guid ecosystemId,
        Guid userId,
        string rawName)
    {
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(ecosystemId, "Ecosystem Id", errors);
        DomainGuard.CheckNotEmpty(userId, nameof(UserId), errors);
        Name? ecosystemName = DomainGuard.Collect(Name.Create(rawName), errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<Ecosystem>(errors);
        }

        var ecosystem = new Ecosystem(
            ecosystemId, userId,
            ecosystemName!,
            createdAt: DateTime.UtcNow);

        return Result<Ecosystem>.Success(ecosystem);
    }

    public Result SetName(string rawName)
    {
        Result<Name> nameResult = Name.Create(rawName);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        EcosystemName = nameResult.Value;

        IncrementVersion();

        return Result.Success();
    }
}
