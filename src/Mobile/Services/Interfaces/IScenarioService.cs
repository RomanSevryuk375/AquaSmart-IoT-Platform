using Mobile.Models;

namespace Mobile.Services.Interfaces;

public interface IScenarioService
{
    Task<List<ScenarioModel>> GetScenariosAsync(Guid aquariumId, CancellationToken cancellationToken = default);
    Task<ScenarioModel> AddScenarioAsync(ScenarioModel scenario, CancellationToken cancellationToken = default);
    Task<bool> ToggleScenarioAsync(Guid scenarioId, CancellationToken cancellationToken = default);
    Task<bool> DeleteScenarioAsync(Guid scenarioId, CancellationToken cancellationToken = default);
    void RemoveScenariosForAquarium(Guid aquariumId);
    int GetScenariosCount(Guid aquariumId);
}
