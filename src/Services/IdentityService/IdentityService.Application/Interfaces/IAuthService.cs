using Contracts.Results;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(
        LoginUserRequestDto loginUser, 
        CancellationToken cancellationToken);

    Task<Result<LoginResponseDto>> LoginWithRefreshTokenAsync(
        RefreshTokenRequestDto request, 
        CancellationToken cancellationToken);

    Task<Result> LogoutAsync(
        CancellationToken cancellationToken);

    Task<Result<LoginResponseDto>> RegisterUserAsync(
        RegisterUserRequestDto registerDto, 
        CancellationToken cancellationToken);
}