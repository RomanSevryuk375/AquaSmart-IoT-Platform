using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using Device.Application.Features.Integrations.ValidateDeviceToken;

namespace Device.Application.Features.Integrations.ValidateDeviceTokenByControllerId;

public sealed record ValidateDeviceTokenByControllerIdQuery : IQuery<Result<ValidateDeviceTokenDto>>
{
    public Guid ControllerId { get; init; }
    public string RawDeviceToken { get; init; } = string.Empty;
}
