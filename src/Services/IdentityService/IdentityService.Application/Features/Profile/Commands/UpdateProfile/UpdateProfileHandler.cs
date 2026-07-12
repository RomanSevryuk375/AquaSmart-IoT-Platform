using Contracts.Constants;
using Contracts.Results;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Features.Profile.Commands.UpdateProfile;

public sealed class UpdateProfileHandler(
    UserManager<User> userManager,
    IUserContext userContext) : IRequestHandler<UpdateProfileCommand, Result>
{
    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(userContext.UserId.ToString());
        if (user is null)
        {
            return Result.Failure(Error.NotFound<User>(ErrorMessages.Identity.UserNotFound));
        }

        Result updateResult = user.UpdateProfile(request.Name, request.PhoneNumber);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        IdentityResult result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            string error = string.Join("; ", result.Errors.Select(x => x.Description));
            return Result.Failure(Error.Conflict(ErrorCodes.Identity.ProfileUpdateFailed, error));
        }

        return Result.Success();
    }
}
