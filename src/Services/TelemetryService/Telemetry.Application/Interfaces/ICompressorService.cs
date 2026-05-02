namespace Telemetry.Application.Interfaces;

public interface ICompressorService
{
    Task CompressToDaysAsync(
        CancellationToken cancellationToken);

    Task CompressToHoursAsync(
        CancellationToken cancellationToken);

    Task CompressToMinutesAsync(
        CancellationToken cancellationToken);
}