using Contracts.Results;

namespace Telemetry.Application.Interfaces;

public interface IOutboxMessageProcessorService
{
    Task<Result> ProcessAsync(
        CancellationToken cancellationToken);
}