// Ignore Spelling: Validator

using Contracts.Results;

namespace Control.Domain.Interfaces;

public interface IHardwareValidator
{
    public Task<Result> ValidateAssignmentAsync(
        Guid sensorId,
        Guid relayId,
        CancellationToken cancellationToken = default);
}
