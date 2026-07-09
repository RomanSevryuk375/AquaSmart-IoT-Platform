using Contracts.Constants;
using Contracts.Results;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Auth.Commands.Login;
using IdentityService.Application.Features.Auth.Commands.Logout;
using IdentityService.Application.Features.Auth.Commands.Refresh;
using IdentityService.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IdentityService.API.Controllers;

[ApiController]
[Route(ApiConstants.Routes.Auth)]
public class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> RegisterAsync(
        [FromBody] RegisterUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand
        {
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Name = request.Name,
            Password = request.Password,
            TimeZone = request.TimeZone
        };

        Result<LoginResponseDto> result = await sender.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            AppendAuthCookies(result.Value);
        }

        return this.ToActionResult(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> LoginAsync(
        [FromBody] LoginUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        Result<LoginResponseDto> result = await sender.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            AppendAuthCookies(result.Value);
        }

        return this.ToActionResult(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponseDto>> RefreshAsync(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RefreshTokenRequestDto? request,
        CancellationToken cancellationToken)
    {
        string? refreshToken = request?.RefreshToken;

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            Request.Cookies.TryGetValue(
                Contracts.Authorization.Extensions.RefreshTokenCookieName,
                out refreshToken);
        }

        var command = new RefreshCommand
        {
            RefreshToken = refreshToken ?? string.Empty
        };

        Result<LoginResponseDto> result = await sender.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            AppendAuthCookies(result.Value);
        }

        return this.ToActionResult(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> LogoutAsync(CancellationToken cancellationToken)
    {
        var command = new LogoutCommand();

        Result result = await sender.Send(command, cancellationToken);

        Response.Cookies.Delete(
            Contracts.Authorization.Extensions.AccessTokenCookieName,
            CreateAccessTokenCookieOptions());

        Response.Cookies.Delete(
            Contracts.Authorization.Extensions.RefreshTokenCookieName,
            CreateRefreshTokenCookieOptions());

        return this.ToActionResult(result);
    }

    private void AppendAuthCookies(LoginResponseDto token)
    {
        Response.Cookies.Append(
            Contracts.Authorization.Extensions.AccessTokenCookieName,
            token.AccessToken,
            CreateAccessTokenCookieOptions());

        Response.Cookies.Append(
            Contracts.Authorization.Extensions.RefreshTokenCookieName,
            token.RefreshToken,
            CreateRefreshTokenCookieOptions());
    }

    private static CookieOptions CreateAccessTokenCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(12),
            Path = "/",
            IsEssential = true
        };
    }

    private static CookieOptions CreateRefreshTokenCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            Path = "/api/identity/v1/auth",
            IsEssential = true
        };
    }
}
