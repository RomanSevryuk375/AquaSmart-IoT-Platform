using Contracts.Extensions;
using Device.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Device.Infrastructure.Extensions;

public sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId => httpContextAccessor.HttpContext?.User.GetUserId()
        ?? Guid.Empty;
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated
        ?? false;
}
