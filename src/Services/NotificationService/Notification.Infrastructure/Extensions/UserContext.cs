using Contracts.Extensions;
using Microsoft.AspNetCore.Http;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Extensions;

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId => httpContextAccessor.HttpContext?.User.GetUserId() ?? Guid.Empty;
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
