using Contracts.Authorization;
using Contracts.Constants;
using Contracts.Results;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Profile.Commands.ChangePassword;
using IdentityService.Application.Features.Profile.Commands.UpdateProfile;
using IdentityService.Application.Features.Profile.Queries.GetMyProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

[Authorize]
[ApiController]
[Route(ApiConstants.Routes.Profiles)]
public class ProfileController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = SubPermissions.AccountView)]
    public async Task<ActionResult<UserProfileResponseDto>> GetMyProfileAsync(CancellationToken cancellationToken)
    {
        var query = new GetMyProfileQuery();
        Result<UserProfileResponseDto> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("me")]
    [Authorize(Policy = SubPermissions.AccountUpdate)]
    public async Task<ActionResult> UpdateMyProfileAsync(
        [FromBody] UpdateProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProfileCommand
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber ?? string.Empty
        };

        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("password")]
    [Authorize(Policy = SubPermissions.AccountUpdate)]
    public async Task<ActionResult> ChangePasswordAsync(
        [FromBody] ChangePasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand
        {
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }
}
