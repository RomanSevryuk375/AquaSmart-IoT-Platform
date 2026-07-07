using Contracts.Exceptions;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Services;

public class UserService(
    UserManager<User> userManager,
    IUserContext userContext) : IUserService
{
    public async Task<UserProfileResponseDto> GetProfileAsync()
    {
        User user = await userManager.FindByIdAsync(userContext.UserId.ToString())
            ?? throw new NotFoundException($"User {userContext.UserId} not found.");

        return new UserProfileResponseDto
        {
            Id = user.Id,
            Email = user.Email!,
            Name = user.Name.Value,
            PhoneNumber = user.PhoneNumber,
            SubscriptionId = user.SubscriptionId,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task UpdateProfileAsync(UpdateProfileRequestDto request, CancellationToken ct)
    {
        User user = await userManager.FindByIdAsync(userContext.UserId.ToString())
            ?? throw new NotFoundException($"User {userContext.UserId} not found.");

        user.UpdateProfile(request.Name, request.PhoneNumber!);
        user.PhoneNumber = request.PhoneNumber;

        IdentityResult result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            string errors = string.Join(", ", result.Errors.Select(x => x.Description));
            throw new DomainValidationException($"Failed to update profile: {errors}");
        }
    }

    public async Task ChangePasswordAsync(ChangePasswordRequestDto request)
    {
        User user = await userManager.FindByIdAsync(userContext.UserId.ToString())
            ?? throw new NotFoundException($"User {userContext.UserId} not found.");

        IdentityResult result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            string errors = string.Join(", ", result.Errors.Select(x => x.Description));
            throw new DomainValidationException($"Failed to change password: {errors}");
        }
    }
}
