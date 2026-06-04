using Contracts.Results;

namespace Control.Application.Interfaces;

public interface IOutboxMessageProcessorService
{
    Task<Result> ProcessAsync(CancellationToken cancellationToken);
}