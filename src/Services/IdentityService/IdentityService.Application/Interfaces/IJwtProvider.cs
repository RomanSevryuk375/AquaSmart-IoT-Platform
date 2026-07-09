using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces;

public interface IJwtProvider
{
    public string GenerateToken(User user, IReadOnlyList<string> permissions);
    public string GenerateRefreshToken();
}
