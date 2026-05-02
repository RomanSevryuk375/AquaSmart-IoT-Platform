using IdentityService.Application.DTOs;

namespace IdentityService.Application.Interfaces;

public interface IUserService
{
    Task ChangePasswordAsync(
        ChangePasswordRequestDto request);

    Task<UserProfileResponseDto> GetProfileAsync();

    Task UpdateProfileAsync(
        UpdateProfileRequestDto request, 
        CancellationToken ct);
}