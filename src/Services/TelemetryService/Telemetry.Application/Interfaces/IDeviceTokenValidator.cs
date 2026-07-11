// Ignore Spelling: Validator

using Contracts.Results;
using Telemetry.Application.DTOs;

namespace Telemetry.Application.Interfaces;

public interface IDeviceTokenValidator
{
    public Task<Result<ValidateResponseDto>> ValidateAsync(
        string macAddress,
        string deviceToken,
        CancellationToken cancellationToken = default);
}
