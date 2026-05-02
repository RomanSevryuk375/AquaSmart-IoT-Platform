using Contracts.Extensions;
using IdentityService.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace IdentityService.Infrastructure.Extensions;

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId => httpContextAccessor.HttpContext?.User.GetUserId() ?? Guid.Empty;
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
