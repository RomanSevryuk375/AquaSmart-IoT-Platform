using Contracts.Results;

namespace Control.Application.Interfaces;

public interface IScheduleProcessor
{
    Task<Result> ProcessAsync(CancellationToken cancellationToken);
}