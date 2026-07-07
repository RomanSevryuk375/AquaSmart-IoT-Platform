using Contracts.Abstractions;

namespace IdentityService.Application.Features.Profile.Commands.ChangePassword;

public sealed record ChangePasswordCommand : ICommand
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
