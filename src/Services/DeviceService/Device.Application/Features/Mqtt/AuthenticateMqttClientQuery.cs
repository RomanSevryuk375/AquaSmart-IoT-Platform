using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;

namespace Device.Application.Features.Mqtt;

public sealed record AuthenticateMqttClientQuery : IQuery<Result<MqttAuthResult>>
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
}
