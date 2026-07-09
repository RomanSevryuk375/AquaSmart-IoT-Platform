using Contracts.Results;

namespace Telemetry.Application.Interfaces;

public interface ICompressorService
{
    public Task<Result> CompressToDaysAsync(
        CancellationToken cancellationToken);

    public Task<Result> CompressToHoursAsync(
        CancellationToken cancellationToken);

    public Task<Result> CompressToMinutesAsync(
        CancellationToken cancellationToken);
}