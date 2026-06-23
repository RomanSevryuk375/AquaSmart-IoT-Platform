using Contracts.Abstractions;

namespace Device.Application.Features.Controllers.Query.GetControllerConfig;

public sealed record GetControllerConfigQuery
    : IQuery<Result<ControllerConfig>>
{
    public string DeviceToken { get; init; } = string.Empty;
    public string MacAddress { get; init; } = string.Empty;
}
