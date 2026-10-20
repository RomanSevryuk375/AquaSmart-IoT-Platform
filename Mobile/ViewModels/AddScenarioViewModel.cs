using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

[QueryProperty(nameof(AquariumIdString), "aquariumId")]
public partial class AddScenarioViewModel : ObservableObject
{
    private readonly IAquariumService _aquariumService;

    [ObservableProperty]
    private string? _aquariumIdString;

    [ObservableProperty]
    private string _scenarioName = string.Empty;

    [ObservableProperty]
    private string _condition = "Если Температура < 24.5°C";

    [ObservableProperty]
    private string _action = "Включить Нагреватель";

    [ObservableProperty]
    private string _frequency = "Постоянно (по триггеру)";

    public AddScenarioViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ScenarioName))
        {
            await Shell.Current.DisplayAlert("Внимание", "Укажите название сценария", "OK");
            return;
        }

        Guid.TryParse(AquariumIdString, out var aqId);

        var scenario = new ScenarioModel
        {
            AquariumId = aqId,
            Name = ScenarioName,
            ConditionText = Condition,
            ActionText = Action,
            FrequencyText = Frequency,
            IsActive = true
        };

        await _aquariumService.AddScenarioAsync(scenario);
        await Shell.Current.DisplayAlert("Успех", $"Сценарий «{ScenarioName}» активирован!", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
