using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;

namespace Device.Application.Features.Mqtt;

public sealed record AuthorizeMqttActionQuery : IQuery<Result<MqttAclResult>>
{
    public string Username { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
}
