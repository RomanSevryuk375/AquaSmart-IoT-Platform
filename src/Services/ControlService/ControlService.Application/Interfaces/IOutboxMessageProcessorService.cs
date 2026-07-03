using Contracts.Results;

namespace Control.Application.Interfaces;

public interface IOutboxMessageProcessorService
{
    public Task<Result> ProcessAsync(CancellationToken cancellationToken);
}