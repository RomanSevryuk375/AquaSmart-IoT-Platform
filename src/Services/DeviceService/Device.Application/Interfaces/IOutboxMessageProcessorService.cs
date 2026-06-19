namespace Device.Application.Interfaces;

public interface IOutboxMessageProcessorService
{
    Task<Result> ProcessAsync(CancellationToken cancellationToken);
}