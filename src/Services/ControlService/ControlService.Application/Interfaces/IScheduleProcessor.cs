using Contracts.Results;

namespace Control.Application.Interfaces;

public interface IScheduleProcessor
{
    public Task<Result> ProcessAsync(CancellationToken cancellationToken);
}