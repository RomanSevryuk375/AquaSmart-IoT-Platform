using IdentityService.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace IdentityService.Application.Interfaces;

public interface ICookieHelpers
{
    public void AppendAuthCookies(HttpContext context, LoginResponseDto token);
    public void ClearAuthCookies(HttpContext context);
}
