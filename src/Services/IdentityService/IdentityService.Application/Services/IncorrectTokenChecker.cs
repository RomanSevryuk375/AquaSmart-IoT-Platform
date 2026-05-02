using IdentityService.Application.Interfaces;
using IdentityService.Domain.Interfaces;

namespace IdentityService.Application.Services;

public class IncorrectTokenChecker(
    IRefreshTokenRepository tokenRepository,
    IUnitOfWork unitOfWork) : IIncorrectTokenChecker
{
    public async Task CheckAndDeleteAsync(CancellationToken cancellationToken)
    {
        await tokenRepository
            .DeleteIncorrectTokensAsync(cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
