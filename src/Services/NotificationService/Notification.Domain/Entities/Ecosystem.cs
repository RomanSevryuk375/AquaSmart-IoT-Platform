using Contracts.Abstractions;
using Contracts.Results;
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
        Result<Name> nameResult = Name.Create(rawName);
        if (nameResult.IsFailure)
        {
            return Result<Ecosystem>.Failure(nameResult.Error);
        }

        var ecosystem = new Ecosystem(
            ecosystemId, userId,
            nameResult.Value,
            createdAt: DateTime.Now);

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
