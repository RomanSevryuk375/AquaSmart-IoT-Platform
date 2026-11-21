using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Device;

public sealed record OtaCheckResponseDto
{
    [JsonPropertyName("has_update")]
    public bool HasUpdate { get; init; }

    [JsonPropertyName("version")]
    public string? Version { get; init; }

    [JsonPropertyName("download_url")]
    public string? DownloadUrl { get; init; }

    [JsonPropertyName("file_hash")]
    public string? FileHash { get; init; }
}
