using Contracts.Abstractions;

namespace IdentityService.Application.Features.Profile.Commands.UpdateProfile;

public sealed record UpdateProfileCommand : ICommand
{
    public string Name { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
}
