using Contracts.Results;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using IdentityService.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Features.Auth.Commands.Login;

public sealed class LoginHandler(
    UserManager<User> userManager,
    ISubscriptionRepository subscriptionRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtProvider jwtProvider,
    IMyHasher myHasher) : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        User? existingUser = await userManager.FindByEmailAsync(request.Email);

        if (existingUser is null || !await userManager.CheckPasswordAsync(existingUser, request.Password))
        {
            return Result<LoginResponseDto>.Failure(Error.Conflict("Auth.Invalid",
                "Invalid credentials."));
        }

        Subscription? subscription = await subscriptionRepository.GetByIdAsync(
            existingUser.SubscriptionId, cancellationToken);
        List<string> permissions = subscription?.Permissions.ToList() ?? [];

        string accessToken = jwtProvider.GenerateToken(existingUser, permissions);
        string rawSecret = jwtProvider.GenerateRefreshToken();
        Result<RefreshToken> tokenResult = RefreshToken.Create(existingUser.Id, myHasher.Generate(rawSecret));

        if (tokenResult.IsFailure)
        {
            return Result<LoginResponseDto>.Failure(tokenResult.Error);
        }

        await refreshTokenRepository.AddAsync(tokenResult.Value, cancellationToken);

        return Result<LoginResponseDto>.Success(new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = $"{tokenResult.Value.Id}.{rawSecret}",
        });
    }
}
