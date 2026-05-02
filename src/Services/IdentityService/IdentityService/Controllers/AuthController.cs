using Contracts.Results;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/identity/v1/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> RegisterAsync(
        [FromBody] RegisterUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await authService
            .RegisterUserAsync(request, cancellationToken);

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
        var result = await authService
            .LoginAsync(request, cancellationToken);

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
        var refreshToken = request?.RefreshToken;
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            Request.Cookies.TryGetValue(
                Contracts.Authorization.Extensions.RefreshTokenCookieName,
                out refreshToken);
        }

        var result = await authService
            .LoginWithRefreshTokenAsync(new RefreshTokenRequestDto
            {
                RefreshToken = refreshToken ?? string.Empty
            }, cancellationToken);

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
        await authService.LogoutAsync(cancellationToken);

        Response.Cookies.Delete(
            Contracts.Authorization.Extensions.AccessTokenCookieName,
            CreateAccessTokenCookieOptions());
        Response.Cookies.Delete(
            Contracts.Authorization.Extensions.RefreshTokenCookieName,
            CreateRefreshTokenCookieOptions());

        return NoContent();
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
