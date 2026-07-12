using Contracts.Constants;
using Contracts.Results;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
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
                ErrorMessages.Identity.InvalidTokenFormat));
        }

        RefreshToken? tokenEntity = await refreshTokenRepository.GetByIdAsync(tokenId, cancellationToken);
        if (tokenEntity is null)
        {
            return Result<LoginResponseDto>.Failure(Error.Validation<RefreshToken>(
                ErrorMessages.Identity.TokenInvalidOrExpired));
        }

        if (tokenEntity.IsUsed)
        {
            await refreshTokenRepository.DeleteTokensByUserIdAsync(tokenEntity.UserId, cancellationToken);
            return Result<LoginResponseDto>.Failure(Error.Conflict(
                ErrorCodes.Identity.TokenReuse,
                ErrorMessages.Identity.TokenReuseDetected));
        }

        if (tokenEntity.IsRevoked ||
            tokenEntity.ExpiredAt < DateTime.UtcNow ||
            !myHasher.Verify(parts[1], tokenEntity.TokenHash))
        {
            return Result<LoginResponseDto>.Failure(Error.Validation<RefreshToken>(
                ErrorMessages.Identity.TokenInvalidOrExpired));
        }

        tokenEntity.MarkAsUsed();

        User? existingUser = await userManager.FindByIdAsync(tokenEntity.UserId.ToString());
        if (existingUser is null)
        {
            return Result<LoginResponseDto>.Failure(Error.NotFound<User>(
                ErrorMessages.Identity.UserNotFound));
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
