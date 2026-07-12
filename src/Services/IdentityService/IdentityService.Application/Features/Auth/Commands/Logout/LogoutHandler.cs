using Contracts.Results;
using IdentityService.Domain.Interfaces;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.Logout;

public sealed class LogoutHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserContext userContext)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await refreshTokenRepository.DeleteTokensByUserIdAsync(userContext.UserId, cancellationToken);

        return Result.Success();
    }
}
