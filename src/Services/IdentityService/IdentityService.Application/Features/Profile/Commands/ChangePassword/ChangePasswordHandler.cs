using Contracts.Constants;
using Contracts.Results;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Features.Profile.Commands.ChangePassword;

public sealed class ChangePasswordHandler(
    UserManager<User> userManager,
    IUserContext userContext) : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(userContext.UserId.ToString());
        if (user is null)
        {
            return Result.Failure(Error.NotFound<User>(
                ErrorMessages.Identity.UserNotFound));
        }

        IdentityResult result = await userManager.ChangePasswordAsync(
            user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            string error = string.Join("; ", result.Errors.Select(x => x.Description));
            return Result.Failure(Error.Validation(ErrorCodes.Identity.PasswordChangeFailed, error));
        }

        return Result.Success();
    }
}
