using Contracts.Events.UserEvents;
using Contracts.Exceptions;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Services;

public class UserService(
    UserManager<UserEntity> userManager,
    IUserContext userContext,
    IPublishEndpoint publishEndpoint) : IUserService
{
    public async Task<UserProfileResponseDto> GetProfileAsync()
    {
        var user = await userManager.FindByIdAsync(userContext.UserId.ToString())
            ?? throw new NotFoundException($"User {userContext.UserId} not found.");

        return new UserProfileResponseDto
        {
            Id = user.Id,
            Email = user.Email!,
            Name = user.Name,
            PhoneNumber = user.PhoneNumber,
            SubscriptionId = user.SubscriptionId,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task UpdateProfileAsync(UpdateProfileRequestDto request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userContext.UserId.ToString())
            ?? throw new NotFoundException($"User {userContext.UserId} not found.");

        user.SetName(request.Name);
        user.PhoneNumber = request.PhoneNumber;

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(x => x.Description));
            throw new DomainValidationException($"Failed to update profile: {errors}");
        }

        await publishEndpoint.Publish(new UserUpdatedEvent
        {
            UserId = user.Id,
            Email = user.Email!,
            Name = user.Name,
            PhoneNumber = user.PhoneNumber!
        }, ct);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequestDto request)
    {
        var user = await userManager.FindByIdAsync(userContext.UserId.ToString())
            ?? throw new NotFoundException($"User {userContext.UserId} not found.");

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(x => x.Description));
            throw new DomainValidationException($"Failed to change password: {errors}");
        }
    }
}
