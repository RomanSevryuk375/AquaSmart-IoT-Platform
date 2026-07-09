using Contracts.Abstractions;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand : ICommand<LoginResponseDto>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
