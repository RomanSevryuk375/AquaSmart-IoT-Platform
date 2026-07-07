using Contracts.Abstractions;
using Contracts.Results;

namespace IdentityService.Domain.Entities;

public sealed class RefreshToken : AggregateRoot, IEntity
{
    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        bool isUsed,
        bool isRevoked,
        DateTime expiredAt,
        DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        IsUsed = isUsed;
        IsRevoked = isRevoked;
        ExpiredAt = expiredAt;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618
    private RefreshToken() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public bool IsUsed { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime ExpiredAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<RefreshToken> Create(Guid userId, string tokenHash)
    {
        if (userId == Guid.Empty)
        {
            return Result<RefreshToken>.Failure(Error.Validation<RefreshToken>(
                "UserId cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return Result<RefreshToken>.Failure(Error.Validation<RefreshToken>(
                "Token hash cannot be empty."));
        }

        var token = new RefreshToken(
            Guid.NewGuid(),
            userId,
            tokenHash,
            isUsed: false,
            isRevoked: false,
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow);

        return Result<RefreshToken>.Success(token);
    }

    public void MarkAsUsed()
    {
        IsUsed = true;
        IncrementVersion();
    }

    public void MarkAsRevoked()
    {
        IsRevoked = true;
        IncrementVersion();
    }
}

