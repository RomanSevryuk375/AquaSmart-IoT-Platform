using Contracts.Results;
using IdentityService.Domain.Interfaces;
using MediatR;

namespace IdentityService.Application.Features.BackgroundJobs.Commands.CleanIncorrectTokens;

public sealed class CleanIncorrectTokensHandler(IRefreshTokenRepository tokenRepository)
    : IRequestHandler<CleanIncorrectTokensCommand, Result>
{
    public async Task<Result> Handle(CleanIncorrectTokensCommand request, CancellationToken cancellationToken)
    {
        await tokenRepository.DeleteIncorrectTokensAsync(cancellationToken);

        return Result.Success();
    }
}
