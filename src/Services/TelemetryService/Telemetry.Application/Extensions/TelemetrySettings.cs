namespace Telemetry.Application.Extensions;

public sealed record TelemetrySettings
{
    public const string SectionName = "TelemetrySettings";
    public int MaxLiveTimeForRawDataInHours { get; init; } = -24;
    public int MaxLiveTimeForMinutesDataInDayes { get; init; } = -7;
    public int MaxLiveTimeForHourseDataInDayes { get; init; } = -90;

}
