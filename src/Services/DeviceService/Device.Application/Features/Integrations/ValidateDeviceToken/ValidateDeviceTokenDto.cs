// Ignore Spelling: Dto

namespace Device.Application.Features.Integrations.ValidateDeviceToken;

public sealed record ValidateDeviceTokenDto
{
    public Guid UserId { get; init; }
    public Guid ControllerId { get; init; }
    public bool IsValid { get; init; }
}
