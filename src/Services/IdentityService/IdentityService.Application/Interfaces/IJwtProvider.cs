using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(UserEntity user, List<string> permissions);
    string GenerateRefreshToken();
}