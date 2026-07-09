using Contracts.Abstractions;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand : ICommand<LoginResponseDto>
{
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
}
