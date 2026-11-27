using Mobile.Models;
using Mobile.Services.Dto.Control;
using Mobile.Services.Interfaces;

namespace Mobile.Services;

public sealed class ScenarioService : IScenarioService
{
    private readonly IApiClient _apiClient;
    private readonly object _syncLock = new();
    private readonly List<ScenarioModel> _scenarios = new();

    public ScenarioService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<ScenarioModel>> GetScenariosAsync(Guid aquariumId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{ApiConfiguration.Routes.AutomationRules}?ecosystemId={aquariumId}";
            var response = await _apiClient.GetAsync<List<AutomationRuleDto>>(route, cancellationToken);
            if (response.IsSuccess && response.Data != null)
            {
                var mapped = response.Data.Select(dto => new ScenarioModel
                {
                    Id = dto.Id,
                    AquariumId = dto.EcosystemId,
                    Name = dto.Name,
                    IsActive = dto.IsActive,
                    ConditionText = dto.Conditions?.Count > 0
                        ? $"{LocalizationService.Instance["Tank_Sensors"]}: > {dto.Conditions[0].Threshold}"
                        : LocalizationService.Instance["Relay_Scheduled"],
                    ActionText = dto.Action == 1 ? LocalizationService.Instance["Scenario_TurnOn"] : LocalizationService.Instance["Scenario_TurnOff"],
                    FrequencyText = LocalizationService.Instance["Relay_AutomaticMode"]
                }).ToList();

                lock (_syncLock)
                {
                    var localOnly = _scenarios
                        .Where(s => s.AquariumId == aquariumId && !mapped.Any(m => m.Id == s.Id))
                        .ToList();

                    _scenarios.RemoveAll(s => s.AquariumId == aquariumId);
                    _scenarios.AddRange(mapped);
                    _scenarios.AddRange(localOnly);

                    return _scenarios.Where(s => s.AquariumId == aquariumId).ToList();
                }
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            return _scenarios.Where(x => x.AquariumId == aquariumId).ToList();
        }
    }

    public async Task<ScenarioModel> AddScenarioAsync(ScenarioModel scenario, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CreateRuleRequestDto(
                EcosystemId: scenario.AquariumId,
                RelayId: Guid.NewGuid(),
                Name: scenario.Name,
                Operator: 1, // AND
                Action: 1,   // SwitchOn
                IsActive: scenario.IsActive,
                Conditions: new List<RuleConditionRequestDto>()
            );

            var response = await _apiClient.PostAsync<CreateRuleRequestDto, Guid>(
                ApiConfiguration.Routes.AutomationRules, request, cancellationToken);

            if (response.IsSuccess && response.Data != Guid.Empty)
            {
                scenario.Id = response.Data;
            }
        }
        catch
        {
        }

        if (scenario.Id == Guid.Empty)
        {
            scenario.Id = Guid.NewGuid();
        }

        lock (_syncLock)
        {
            var existingIdx = _scenarios.FindIndex(s => s.Id == scenario.Id);
            if (existingIdx >= 0)
            {
                _scenarios[existingIdx] = scenario;
            }
            else
            {
                _scenarios.Add(scenario);
            }
        }

        return scenario;
    }

    public Task<bool> ToggleScenarioAsync(Guid scenarioId, CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            var sc = _scenarios.FirstOrDefault(x => x.Id == scenarioId);
            if (sc != null)
            {
                sc.IsActive = !sc.IsActive;
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }

    public async Task<bool> DeleteScenarioAsync(Guid scenarioId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{ApiConfiguration.Routes.AutomationRules}/{scenarioId}";
            await _apiClient.DeleteAsync(route, cancellationToken);
        }
        catch
        {
        }

        lock (_syncLock)
        {
            _scenarios.RemoveAll(s => s.Id == scenarioId);
        }

        return true;
    }

    public void RemoveScenariosForAquarium(Guid aquariumId)
    {
        lock (_syncLock)
        {
            _scenarios.RemoveAll(s => s.AquariumId == aquariumId);
        }
    }

    public int GetScenariosCount(Guid aquariumId)
    {
        lock (_syncLock)
        {
            return _scenarios.Count(s => s.AquariumId == aquariumId);
        }
    }
}
