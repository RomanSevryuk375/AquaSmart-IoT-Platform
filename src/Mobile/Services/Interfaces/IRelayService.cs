using Mobile.Models;

namespace Mobile.Services.Interfaces;

public interface IRelayService
{
    Task<List<RelayModel>> GetRelaysAsync(Guid aquariumId, CancellationToken cancellationToken = default);
    Task<RelayModel> AddRelayAsync(RelayModel relay, CancellationToken cancellationToken = default);
    Task<bool> ToggleRelayAsync(Guid relayId, CancellationToken cancellationToken = default);
    Task<bool> DeleteRelayAsync(Guid relayId, CancellationToken cancellationToken = default);
    void RemoveRelaysForAquarium(Guid aquariumId);
    int GetRelaysCount(Guid aquariumId);
    RelayModel? GetFirstRelayForAquarium(Guid aquariumId);
}
