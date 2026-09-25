using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Authorization;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace IdentityService.Application.Features.Auth.Commands.TelegramLogin;

public sealed class TelegramLoginHandler(
    IUserRepository userRepository,
    ISubscriptionRepository subscriptionRepository,
    IJwtProvider jwtProvider,
    IOptions<JwtOptions> jwtOptions) : IRequestHandler<TelegramLoginCommand, Result<TelegramLoginResponseDto>>
{
    public async Task<Result<TelegramLoginResponseDto>> Handle(
        TelegramLoginCommand request,
        CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByTelegramChatIdAsync(request.ChatId, cancellationToken);
        if (user is null)
        {
            return Result<TelegramLoginResponseDto>.Failure(Error.NotFound<User>(
                $"User with Telegram Chat ID '{request.ChatId}' not found or not linked."));
        }

        Subscription? subscription = await subscriptionRepository.GetByIdAsync(
            user.SubscriptionId, cancellationToken);
        List<string> permissions = subscription?.Permissions.ToList() ?? [];

        string accessToken = jwtProvider.GenerateToken(user, permissions);

        return Result<TelegramLoginResponseDto>.Success(new TelegramLoginResponseDto
        {
            AccessToken = accessToken,
            ExpiresHours = jwtOptions.Value.ExpiresHours,
        });
    }
}
