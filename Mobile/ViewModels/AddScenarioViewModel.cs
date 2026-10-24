using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

[QueryProperty(nameof(AquariumIdString), "aquariumId")]
public partial class AddScenarioViewModel : ObservableObject
{
    private readonly IAquariumService _aquariumService;

    [ObservableProperty]
    private string? _aquariumIdString;

    [ObservableProperty]
    private string _scenarioName = "Поддержание температуры";

    [ObservableProperty]
    private string _selectedSensor = "Температура воды";

    [ObservableProperty]
    private string _selectedOperator = "<";

    [ObservableProperty]
    private string _thresholdValue = "24.5";

    [ObservableProperty]
    private string _selectedAction = "Включить";

    [ObservableProperty]
    private string _selectedRelay = "Нагреватель";

    [ObservableProperty]
    private string _selectedDuration = "Постоянно";

    [ObservableProperty]
    private bool _isActive = true;

    // View States matching Figma
    [ObservableProperty]
    private bool _isFormVisible = true;

    [ObservableProperty]
    private bool _isSuccessVisible = false;

    [ObservableProperty]
    private bool _isCancelDialogOpen = false;

    // Success card info
    [ObservableProperty]
    private string _createdScenarioTitle = "Поддержание температуры";

    [ObservableProperty]
    private string _createdScenarioSubtitle = "Если Температура < 24.5°C • Включить Нагреватель";

    [ObservableProperty]
    private string _createdScenarioStatus = "Активно";

    public string SensorUnit => SelectedSensor switch
    {
        "Температура воды" => "°C",
        "Кислотность pH" => "pH",
        "Жесткость TDS" => "ppm",
        "Уровень воды" => "см",
        "Редокс ОВП" => "mV",
        _ => ""
    };

    partial void OnSelectedSensorChanged(string value)
    {
        OnPropertyChanged(nameof(SensorUnit));
        if (value.Contains("pH") && ThresholdValue == "24.5") ThresholdValue = "6.8";
        else if (value.Contains("TDS") && ThresholdValue == "24.5") ThresholdValue = "150";
        else if (value.Contains("Уровень") && ThresholdValue == "24.5") ThresholdValue = "10";
    }

    public AddScenarioViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;
    }

    [RelayCommand]
    private void SelectOperator(string op)
    {
        SelectedOperator = op;
    }

    [RelayCommand]
    private void SelectAction(string action)
    {
        SelectedAction = action;
    }

    [RelayCommand]
    private void SelectDuration(string duration)
    {
        SelectedDuration = duration;
    }

    [RelayCommand]
    private void SelectPresetThreshold(string val)
    {
        ThresholdValue = val;
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

        var condition = $"Если {SelectedSensor} {SelectedOperator} {ThresholdValue}{SensorUnit}";
        var action = $"{SelectedAction} {SelectedRelay} ({SelectedDuration})";

        var scenario = new ScenarioModel
        {
            AquariumId = aqId,
            Name = ScenarioName.Trim(),
            ConditionText = condition,
            ActionText = action,
            FrequencyText = SelectedDuration,
            IsActive = IsActive
        };

        await _aquariumService.AddScenarioAsync(scenario);

        CreatedScenarioTitle = ScenarioName.Trim();
        CreatedScenarioSubtitle = $"{condition} • {action}";
        CreatedScenarioStatus = IsActive ? "Активно" : "Отключено";

        IsFormVisible = false;
        IsCancelDialogOpen = false;
        IsSuccessVisible = true;
    }

    [RelayCommand]
    private void PromptCancel()
    {
        IsCancelDialogOpen = true;
    }

    [RelayCommand]
    private void ResumeEditing()
    {
        IsCancelDialogOpen = false;
    }

    [RelayCommand]
    private async Task ConfirmCancelAsync()
    {
        IsCancelDialogOpen = false;
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task ReturnToAquariumAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private void AddAnotherScenario()
    {
        ScenarioName = string.Empty;
        ThresholdValue = "24.5";
        IsSuccessVisible = false;
        IsCancelDialogOpen = false;
        IsFormVisible = true;
    }
}
