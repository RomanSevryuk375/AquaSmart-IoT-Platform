using DTOs;

namespace Interfaces;

/// <summary>
/// HTTP client contract for ControlService (accessed via API Gateway with user Bearer JWT).
/// Used to populate ecosystem selection keyboards.
/// </summary>
public interface IControlApiClient
{
    /// <summary>
    /// Returns all ecosystems that belong to the user identified by <paramref name="chatId"/>.
    /// </summary>
    public Task<IReadOnlyList<EcosystemDto>> GetUserEcosystemsAsync(
        long chatId,
        CancellationToken cancellationToken = default);
}
