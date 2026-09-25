using BuildingBlocks.Domain.Abstractions;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.Features.Auth.Commands.TelegramLogin;

public sealed record TelegramLoginCommand : ICommand<TelegramLoginResponseDto>
{
    public long ChatId { get; init; }
}
