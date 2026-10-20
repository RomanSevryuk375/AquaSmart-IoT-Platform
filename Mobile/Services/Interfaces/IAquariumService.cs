namespace Mobile.Services.Interfaces;

public interface IAquariumService
{
    Task<List<AquariumModel>> GetAquariumsAsync();
    Task<AquariumModel?> GetAquariumByIdAsync(Guid id);
    Task<AquariumModel> AddAquariumAsync(AquariumModel model);
    Task<List<SensorModel>> GetSensorsAsync(Guid aquariumId);
    Task<SensorModel> AddSensorAsync(SensorModel sensor);
    Task<List<RelayModel>> GetRelaysAsync(Guid aquariumId);
    Task<RelayModel> AddRelayAsync(RelayModel relay);
    Task<bool> ToggleRelayAsync(Guid relayId);
    Task<List<ScenarioModel>> GetScenariosAsync(Guid aquariumId);
    Task<ScenarioModel> AddScenarioAsync(ScenarioModel scenario);
    Task<bool> ToggleScenarioAsync(Guid scenarioId);
}
