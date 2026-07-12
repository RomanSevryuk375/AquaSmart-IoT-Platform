using Contracts.Constants;
using Contracts.Enums;
using Contracts.Results;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Features.Auth.Commands.Register;

public sealed class RegisterHandler(
    UserManager<User> userManager,
    ISubscriptionRepository subscriptionRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtProvider jwtProvider,
    IMyHasher myHasher) : IRequestHandler<RegisterCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            return Result<LoginResponseDto>.Failure(Error.Conflict(
                ErrorCodes.Identity.EmailBusy,
                ErrorMessages.Identity.EmailInUse));
        }

        Result<User> createdResult = User.Create(
            userId: NewId.NextGuid(),
            request.Name,
            request.Email,
            request.PhoneNumber,
            subscriptionId: Guid.Parse(SubscriptionType.Free),
            request.TimeZone);

        if (createdResult.IsFailure)
        {
            return Result<LoginResponseDto>.Failure(createdResult.Error);
        }

        User user = createdResult.Value;

        IdentityResult identityResult = await userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            string error = string.Join("; ", identityResult.Errors.Select(x => x.Description));
            return Result<LoginResponseDto>.Failure(Error.Conflict(
                ErrorCodes.Identity.RegisterFailure,
                error));
        }

        Subscription? subscription = await subscriptionRepository.GetByIdAsync(
            user.SubscriptionId, cancellationToken);
        List<string> permissions = subscription?.Permissions.ToList() ?? [];

        string accessToken = jwtProvider.GenerateToken(user, permissions);

        string rawSecret = jwtProvider.GenerateRefreshToken();
        Result<RefreshToken> tokenResult = RefreshToken.Create(user.Id, myHasher.Generate(rawSecret));

        if (tokenResult.IsFailure)
        {
            return Result<LoginResponseDto>.Failure(tokenResult.Error);
        }

        await refreshTokenRepository.AddAsync(tokenResult.Value, cancellationToken);

        return Result<LoginResponseDto>.Success(new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = $"{tokenResult.Value.Id}.{rawSecret}"
        });
    }
}
