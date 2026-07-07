using System;
using Contracts.Results;
using Identity.TestShared.Constants;
using IdentityService.Domain.Entities;

namespace Identity.TestShared.Builders;

public class RefreshTokenBuilder
{
    private Guid _userId = IdentityTestConstants.UserId;
    private string _tokenHash = "test-token-hash-12345";
    private bool _isUsed;
    private bool _isRevoked;

    public RefreshTokenBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public RefreshTokenBuilder WithTokenHash(string tokenHash)
    {
        _tokenHash = tokenHash;
        return this;
    }

    public RefreshTokenBuilder AsUsed()
    {
        _isUsed = true;
        return this;
    }

    public RefreshTokenBuilder AsRevoked()
    {
        _isRevoked = true;
        return this;
    }

    public RefreshToken Build()
    {
        Result<RefreshToken> result = RefreshToken.Create(_userId, _tokenHash);

        if (result.IsFailure)
        {
            throw new ArgumentException($"RefreshTokenBuilder failed: {result.Error.Message}");
        }

        RefreshToken token = result.Value;

        if (_isUsed)
        {
            token.MarkAsUsed();
        }

        if (_isRevoked)
        {
            token.MarkAsRevoked();
        }

        token.ClearDomainEvents();
        return token;
    }
}
