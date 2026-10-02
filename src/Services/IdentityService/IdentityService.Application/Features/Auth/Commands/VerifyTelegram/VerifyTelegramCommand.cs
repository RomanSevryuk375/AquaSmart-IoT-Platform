using BuildingBlocks.Domain.Abstractions;

namespace IdentityService.Application.Features.Auth.Commands.VerifyTelegram;

public sealed record VerifyTelegramCommand : ICommand
{
    public string Token { get; init; } = string.Empty;
    public long ChatId { get; init; }
}
