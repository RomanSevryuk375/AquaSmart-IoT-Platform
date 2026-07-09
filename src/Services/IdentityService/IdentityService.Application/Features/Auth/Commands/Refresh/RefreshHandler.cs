using Contracts.Results;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using IdentityService.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Features.Auth.Commands.Refresh;

public sealed class RefreshHandler(
    UserManager<User> userManager,
    ISubscriptionRepository subscriptionRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtProvider jwtProvider,
    IMyHasher myHasher) : IRequestHandler<RefreshCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        string[] parts = request.RefreshToken.Split('.');
        if (parts.Length != 2 || !Guid.TryParse(parts[0], out Guid tokenId))
        {
            return Result<LoginResponseDto>.Failure(Error.Validation<RefreshToken>(
                "Invalid token format."));
        }

        RefreshToken? tokenEntity = await refreshTokenRepository.GetByIdAsync(tokenId, cancellationToken);
        if (tokenEntity is null)
        {
            return Result<LoginResponseDto>.Failure(Error.Validation<RefreshToken>(
                "Token is invalid or expired."));
        }

        if (tokenEntity.IsUsed)
        {
            await refreshTokenRepository.DeleteTokensByUserIdAsync(tokenEntity.UserId, cancellationToken);
            return Result<LoginResponseDto>.Failure(Error.Conflict("Security.Breach",
                "Token reuse detected. All sessions revoked."));
        }

        if (tokenEntity.IsRevoked ||
            tokenEntity.ExpiredAt < DateTime.UtcNow ||
            !myHasher.Verify(parts[1], tokenEntity.TokenHash))
        {
            return Result<LoginResponseDto>.Failure(Error.Validation<RefreshToken>(
                "Token is invalid or expired."));
        }

        tokenEntity.MarkAsUsed();

        User? existingUser = await userManager.FindByIdAsync(tokenEntity.UserId.ToString());
        if (existingUser is null)
        {
            return Result<LoginResponseDto>.Failure(Error.NotFound<User>(
                "User not found."));
        }

        Subscription? subscription = await subscriptionRepository.GetByIdAsync(
            existingUser.SubscriptionId, cancellationToken);
        List<string> permissions = subscription?.Permissions.ToList() ?? [];

        string accessToken = jwtProvider.GenerateToken(existingUser, permissions);

        string rawSecret = jwtProvider.GenerateRefreshToken();
        Result<RefreshToken> newTokenResult = RefreshToken.Create(existingUser.Id, myHasher.Generate(rawSecret));

        if (newTokenResult.IsFailure)
        {
            return Result<LoginResponseDto>.Failure(newTokenResult.Error);
        }

        await refreshTokenRepository.AddAsync(newTokenResult.Value, cancellationToken);

        return Result<LoginResponseDto>.Success(new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = $"{newTokenResult.Value.Id}.{rawSecret}",
        });
    }
}
