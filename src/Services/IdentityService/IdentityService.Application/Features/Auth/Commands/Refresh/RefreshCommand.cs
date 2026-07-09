using Contracts.Abstractions;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.Features.Auth.Commands.Refresh;

public sealed record RefreshCommand : ICommand<LoginResponseDto>
{
    public string RefreshToken { get; init; } = string.Empty;
}
