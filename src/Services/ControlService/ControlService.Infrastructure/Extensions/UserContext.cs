using Contracts.Extensions;
using Control.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Control.Infrastructure.Extensions;

public sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId => httpContextAccessor.HttpContext?.User.GetUserId() ?? Guid.Empty;
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}