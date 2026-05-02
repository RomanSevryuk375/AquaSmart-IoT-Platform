using Contracts.Authorization;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

[Authorize] 
[ApiController]
[Route("api/identity/v1/profile")] 
public class ProfileController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = SubPermissions.AccountView)]
    public async Task<ActionResult<UserProfileResponseDto>> GetMyProfileAsync()
    {
        var profile = await userService.GetProfileAsync();

        return Ok(profile);
    }

    [HttpPut("me")]
    [Authorize(Policy = SubPermissions.AccountUpdate)]
    public async Task<ActionResult> UpdateMyProfileAsync(
        [FromBody] UpdateProfileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await userService.UpdateProfileAsync(request, cancellationToken);

        return NoContent();
    }

    [HttpPost("password")]
    [Authorize(Policy = SubPermissions.AccountUpdate)]
    public async Task<ActionResult> ChangePasswordAsync(
        [FromBody] ChangePasswordRequestDto request)
    {
        await userService.ChangePasswordAsync(request);

        return NoContent();
    }
}
