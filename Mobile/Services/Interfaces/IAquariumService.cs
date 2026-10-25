namespace Mobile.Services.Interfaces;

public interface IAquariumService
{
    Task<List<AquariumModel>> GetAquariumsAsync(CancellationToken cancellationToken = default);
    Task<AquariumModel?> GetAquariumByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AquariumModel> AddAquariumAsync(AquariumModel model, CancellationToken cancellationToken = default);
    Task<AquariumModel> UpdateAquariumAsync(AquariumModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteAquariumAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SensorModel>> GetSensorsAsync(Guid aquariumId, CancellationToken cancellationToken = default);
    Task<SensorModel> AddSensorAsync(SensorModel sensor, CancellationToken cancellationToken = default);
    Task<List<RelayModel>> GetRelaysAsync(Guid aquariumId, CancellationToken cancellationToken = default);
    Task<RelayModel> AddRelayAsync(RelayModel relay, CancellationToken cancellationToken = default);
    Task<bool> ToggleRelayAsync(Guid relayId, CancellationToken cancellationToken = default);
    Task<List<ScenarioModel>> GetScenariosAsync(Guid aquariumId, CancellationToken cancellationToken = default);
    Task<ScenarioModel> AddScenarioAsync(ScenarioModel scenario, CancellationToken cancellationToken = default);
    Task<bool> ToggleScenarioAsync(Guid scenarioId, CancellationToken cancellationToken = default);
}
