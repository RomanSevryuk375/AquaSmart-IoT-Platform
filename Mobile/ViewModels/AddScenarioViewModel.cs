using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services;
using Mobile.Services.Interfaces;

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
    private string _selectedSensor = LocalizationService.Instance["Scenario_Trigger_Temp"];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOperatorLessThan))]
    [NotifyPropertyChangedFor(nameof(IsOperatorLessThanOrEqual))]
    [NotifyPropertyChangedFor(nameof(IsOperatorEqual))]
    [NotifyPropertyChangedFor(nameof(IsOperatorGreaterThanOrEqual))]
    [NotifyPropertyChangedFor(nameof(IsOperatorGreaterThan))]
    private string _selectedOperator = "<";

    public bool IsOperatorLessThan => SelectedOperator == "<";
    public bool IsOperatorLessThanOrEqual => SelectedOperator == "<=";
    public bool IsOperatorEqual => SelectedOperator == "=";
    public bool IsOperatorGreaterThanOrEqual => SelectedOperator == ">=";
    public bool IsOperatorGreaterThan => SelectedOperator == ">";

    [ObservableProperty]
    private string _thresholdValue = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsActionTurnOn))]
    [NotifyPropertyChangedFor(nameof(IsActionTurnOff))]
    [NotifyPropertyChangedFor(nameof(IsActionToggle))]
    private string _selectedAction = LocalizationService.Instance["Scenario_TurnOn"];

    public bool IsActionTurnOn => SelectedAction == LocalizationService.Instance["Scenario_TurnOn"] || SelectedAction is "Включить" or "Turn On" or "turn_on";
    public bool IsActionTurnOff => SelectedAction == LocalizationService.Instance["Scenario_TurnOff"] || SelectedAction is "Выключить" or "Turn Off" or "turn_off";
    public bool IsActionToggle => SelectedAction == LocalizationService.Instance["Scenario_Toggle"] || SelectedAction is "Переключить" or "Toggle" or "toggle";

    [ObservableProperty]
    private string _selectedRelay = LocalizationService.Instance["Scenario_Device_Heater"];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDurationContinuous))]
    [NotifyPropertyChangedFor(nameof(IsDuration15Min))]
    [NotifyPropertyChangedFor(nameof(IsDuration30Min))]
    [NotifyPropertyChangedFor(nameof(IsDuration1Hour))]
    private string _selectedDuration = LocalizationService.Instance["Scenario_Continuous"];

    public bool IsDurationContinuous => SelectedDuration == LocalizationService.Instance["Scenario_Continuous"] || SelectedDuration is "Постоянно" or "Continuous" or "continuous";
    public bool IsDuration15Min => SelectedDuration == LocalizationService.Instance["Scenario_15Min"] || SelectedDuration is "15 мин" or "15 min" or "15_min" or "15";
    public bool IsDuration30Min => SelectedDuration == LocalizationService.Instance["Scenario_30Min"] || SelectedDuration is "30 мин" or "30 min" or "30_min" or "30";
    public bool IsDuration1Hour => SelectedDuration == LocalizationService.Instance["Scenario_1Hour"] || SelectedDuration is "1 час" or "1 hour" or "1_hour" or "60";

    public List<string> TriggerTypes => new()
    {
        LocalizationService.Instance["Scenario_Trigger_Temp"],
        LocalizationService.Instance["Scenario_Trigger_Ph"],
        LocalizationService.Instance["Scenario_Trigger_Tds"],
        LocalizationService.Instance["Scenario_Trigger_Level"],
        LocalizationService.Instance["Scenario_Trigger_Orp"]
    };

    public List<string> TargetDevices => new()
    {
        LocalizationService.Instance["Scenario_Device_Heater"],
        LocalizationService.Instance["Scenario_Device_Filter"],
        LocalizationService.Instance["Scenario_Device_Light"],
        LocalizationService.Instance["Scenario_Device_Aerator"],
        LocalizationService.Instance["Scenario_Device_Co2"],
        LocalizationService.Instance["Scenario_Device_Feeder"]
    };

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
    private string _createdScenarioTitle = LocalizationService.Instance.IsEnglish ? "Temperature Maintenance" : "Поддержание температуры";

    [ObservableProperty]
    private string _createdScenarioSubtitle = LocalizationService.Instance.IsEnglish ? "If Temperature < 24.5°C • Turn On Heater" : "Если Температура < 24.5°C • Включить Нагреватель";

    [ObservableProperty]
    private string _createdScenarioStatus = LocalizationService.Instance["Scenario_ActiveStatus"];

    public string SensorUnit => SelectedSensor switch
    {
        "Температура воды" or "Water Temperature" => "°C",
        "Кислотность pH" or "pH Acidity" => "pH",
        "Жесткость TDS" or "TDS Hardness" => "ppm",
        "Уровень воды" or "Water Level" => LocalizationService.Instance.IsEnglish ? "cm" : "см",
        "Редокс ОВП" or "Redox ORP" => "mV",
        _ => ""
    };

    partial void OnSelectedSensorChanged(string value)
    {
        OnPropertyChanged(nameof(SensorUnit));
        if ((value.Contains("pH") || value.Contains("Кислотность")) && ThresholdValue == "24.5") ThresholdValue = "6.8";
        else if ((value.Contains("TDS") || value.Contains("Жесткость")) && ThresholdValue == "24.5") ThresholdValue = "150";
        else if ((value.Contains("Уровень") || value.Contains("Level")) && ThresholdValue == "24.5") ThresholdValue = "10";
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
        SelectedAction = action switch
        {
            "turn_off" or "Выключить" or "Turn Off" => LocalizationService.Instance["Scenario_TurnOff"],
            "toggle" or "Переключить" or "Toggle" => LocalizationService.Instance["Scenario_Toggle"],
            _ => LocalizationService.Instance["Scenario_TurnOn"]
        };
    }

    [RelayCommand]
    private void SelectDuration(string duration)
    {
        SelectedDuration = duration switch
        {
            "15" or "15_min" or "15 мин" or "15 min" or "15 минут" => LocalizationService.Instance["Scenario_15Min"],
            "30" or "30_min" or "30 мин" or "30 min" or "30 минут" => LocalizationService.Instance["Scenario_30Min"],
            "60" or "1_hour" or "1 час" or "1 hour" => LocalizationService.Instance["Scenario_1Hour"],
            _ => LocalizationService.Instance["Scenario_Continuous"]
        };
    }

    [RelayCommand]
    private void SelectPresetThreshold(string val)
    {
        ThresholdValue = val;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var atWord = LocalizationService.Instance.IsEnglish ? "on" : "при";
        var scenarioName = string.IsNullOrWhiteSpace(ScenarioName) 
            ? $"{SelectedAction} {SelectedRelay} {atWord} {SelectedSensor}" 
            : ScenarioName.Trim();

        var thresh = string.IsNullOrWhiteSpace(ThresholdValue) 
            ? (SelectedSensor.Contains("pH") ? "7.0" : "24.5") 
            : ThresholdValue.Trim();

        Guid.TryParse(AquariumIdString, out var aqId);

        var ifWord = LocalizationService.Instance["Scenario_If"];
        var condition = $"{ifWord} {SelectedSensor} {SelectedOperator} {thresh}{SensorUnit}";
        var action = $"{SelectedAction} {SelectedRelay} ({SelectedDuration})";

        var scenario = new ScenarioModel
        {
            AquariumId = aqId,
            Name = scenarioName,
            ConditionText = condition,
            ActionText = action,
            FrequencyText = SelectedDuration,
            IsActive = IsActive
        };

        await _aquariumService.AddScenarioAsync(scenario);

        CreatedScenarioTitle = scenarioName;
        CreatedScenarioSubtitle = $"{condition} • {action}";
        CreatedScenarioStatus = IsActive 
            ? LocalizationService.Instance["Scenario_ActiveStatus"] 
            : LocalizationService.Instance["Scenario_DisabledStatus"];

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
