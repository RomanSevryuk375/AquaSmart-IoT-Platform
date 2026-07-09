using Contracts.Results;

namespace Telemetry.Application.Interfaces;

public interface IOutboxMessageProcessorService
{
    public Task<Result> ProcessAsync(CancellationToken cancellationToken = default);
}
