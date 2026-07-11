using Contracts.Abstractions;

namespace Device.Application.Features.Integrations.ValidateDeviceToken;

public sealed record ValidateDeviceTokenQuery
    : IQuery<Result<ValidateDeviceTokenDto>>
{
    public string MacAddress { get; set; } = string.Empty;
    public string RawDeviceToken { get; set; } = string.Empty;
}
