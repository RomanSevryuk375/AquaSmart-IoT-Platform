using Contracts.Results;

namespace Telemetry.Application.Interfaces;

public interface ICompressorService
{
    Task<Result> CompressToDaysAsync(
        CancellationToken cancellationToken);

    Task<Result> CompressToHoursAsync(
        CancellationToken cancellationToken);

    Task<Result> CompressToMinutesAsync(
        CancellationToken cancellationToken);
}