using BuildingBlocks.Presentation.Constants;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;

namespace IdentityService.API.Endpoints.Auth;

internal class CookieHelpers(IWebHostEnvironment environment) : ICookieHelpers
{
    public void AppendAuthCookies(HttpContext context, LoginResponseDto token)
    {
        context.Response.Cookies.Append(
            AuthConstants.AccessTokenCookieName,
            token.AccessToken,
            CreateAccessTokenCookieOptions());

        context.Response.Cookies.Append(
            AuthConstants.RefreshTokenCookieName,
            token.RefreshToken,
            CreateRefreshTokenCookieOptions());
    }

    public void ClearAuthCookies(HttpContext context)
    {
        context.Response.Cookies.Delete(
            AuthConstants.AccessTokenCookieName,
            CreateAccessTokenCookieOptions());

        context.Response.Cookies.Delete(
            AuthConstants.RefreshTokenCookieName,
            CreateRefreshTokenCookieOptions());
    }

    private CookieOptions CreateAccessTokenCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = !environment.IsDevelopment(),
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddHours(12),
        Path = "/",
        IsEssential = true
    };

    private CookieOptions CreateRefreshTokenCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = !environment.IsDevelopment(),
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddDays(30),
        Path = "/api/identity/v1/auth",
        IsEssential = true
    };
}
