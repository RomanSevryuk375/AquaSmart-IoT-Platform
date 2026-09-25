using BuildingBlocks.Domain.Abstractions;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.Features.Auth.Commands.GenerateTelegramLink;

public sealed record GenerateTelegramLinkCommand
    : ICommand<TelegramLinkTokenResponseDto>, IUserBoundRequest
{
    public Guid UserId { get; init; }
}
