using Contracts.Results;
using MediatR;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Features.Ecosystems.Commands.SyncEcosystemDeleted;

public sealed class SyncEcosystemDeletedHandler(IEcosystemRepository ecosystemRepository)
    : IRequestHandler<SyncEcosystemDeletedCommand, Result>
{
    public async Task<Result> Handle(SyncEcosystemDeletedCommand request, CancellationToken cancellationToken)
    {
        await ecosystemRepository.DeleteAsync(request.EcosystemId, cancellationToken);

        return Result.Success();
    }
}
