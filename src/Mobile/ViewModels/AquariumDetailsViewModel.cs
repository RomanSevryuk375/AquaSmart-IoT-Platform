using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services;
using Mobile.Services.Dto.Control;
using Mobile.Services.Dto.Notification;
using Mobile.Services.Dto.Telemetry;
using Mobile.Services.Interfaces;
using Mobile.Views;

namespace Mobile.ViewModels;

[QueryProperty(nameof(AquariumIdString), "aquariumId")]
public partial class AquariumDetailsViewModel : ObservableObject
{
    private readonly IAquariumService _aquariumService;
    private readonly ITelemetryHubService _telemetryHubService;
    private readonly ISecurityService _securityService;
    private Guid? _currentSubscribedId;

    [ObservableProperty]
    private string? _aquariumIdString;

    [ObservableProperty]
    private AquariumModel? _aquarium;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSensorsTabSelected))]
    [NotifyPropertyChangedFor(nameof(IsRelaysTabSelected))]
    [NotifyPropertyChangedFor(nameof(IsScenariosTabSelected))]
    [NotifyPropertyChangedFor(nameof(IsLogsTabSelected))]
    private int _selectedTabIndex = 0; // 0: Sensors, 1: Relays, 2: Scenarios, 3: Logs

    public bool IsSensorsTabSelected => SelectedTabIndex == 0;
    public bool IsRelaysTabSelected => SelectedTabIndex == 1;
    public bool IsScenariosTabSelected => SelectedTabIndex == 2;
    public bool IsLogsTabSelected => SelectedTabIndex == 3;

    [ObservableProperty]
    private ObservableCollection<SensorModel> _sensors = new();

    [ObservableProperty]
    private ObservableCollection<RelayModel> _relays = new();

    [ObservableProperty]
    private ObservableCollection<ScenarioModel> _scenarios = new();

    [ObservableProperty]
    private ObservableCollection<MaintenanceLogDto> _maintenanceLogs = new();

    [ObservableProperty]
    private ObservableCollection<ScheduleDto> _schedules = new();

    [ObservableProperty]
    private VacationModeDto? _activeVacationMode;

    [ObservableProperty]
    private bool _isVacationModeActive;

    [ObservableProperty]
    private string _vacationModeText = LocalizationService.Instance["Tank_VacationDisabled"];

    [ObservableProperty]
    private bool _hasNoSensors;

    [ObservableProperty]
    private bool _hasNoRelays;

    [ObservableProperty]
    private bool _hasNoScenarios;

    [ObservableProperty]
    private bool _hasNoLogs;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isLiveTelemetryActive = true;

    public AquariumDetailsViewModel(
        IAquariumService aquariumService,
        ITelemetryHubService telemetryHubService,
        ISecurityService securityService)
    {
        _aquariumService = aquariumService;
        _telemetryHubService = telemetryHubService;
        _securityService = securityService;
        _telemetryHubService.RawTelemetryReceived += OnRawTelemetryReceived;
    }

    private void OnRawTelemetryReceived(object? sender, TelemetryRawChartPointDto point)
    {
        if (point == null) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var sensor = Sensors.FirstOrDefault(s => s.Id == point.SensorId);
            if (sensor != null)
            {
                sensor.Value = Math.Round(point.Value, 2);
                sensor.LastUpdated = point.RecordedAt.ToLocalTime();
            }
        });
    }

    async partial void OnAquariumIdStringChanged(string? value)
    {
        if (Guid.TryParse(value, out var id))
        {
            await LoadAsync(id);
        }
    }

    public async Task LoadAsync(Guid id)
    {
        IsBusy = true;

        if (_currentSubscribedId.HasValue && _currentSubscribedId.Value != id)
        {
            _ = _telemetryHubService.UnsubscribeFromEcosystemAsync(_currentSubscribedId.Value);
        }
        _currentSubscribedId = id;
        _ = _telemetryHubService.SubscribeToEcosystemAsync(id);
        Aquarium = await _aquariumService.GetAquariumByIdAsync(id);
        if (Aquarium != null)
        {
            if (string.IsNullOrWhiteSpace(Aquarium.TankTypeSubtitle) || Aquarium.TankTypeSubtitle == "Ёмкость" || Aquarium.TankTypeSubtitle == "Tank")
            {
                if (Aquarium.SpeciesInfo.Contains("Террариум", StringComparison.OrdinalIgnoreCase) || Aquarium.SpeciesInfo.Contains("Terrarium", StringComparison.OrdinalIgnoreCase)) Aquarium.TankTypeSubtitle = LocalizationService.Instance["TankType_Terrarium"];
                else if (Aquarium.SpeciesInfo.Contains("Гроубокс", StringComparison.OrdinalIgnoreCase) || Aquarium.SpeciesInfo.Contains("Growbox", StringComparison.OrdinalIgnoreCase)) Aquarium.TankTypeSubtitle = LocalizationService.Instance["TankType_Growbox"];
                else if (Aquarium.SpeciesInfo.Contains("Флорариум", StringComparison.OrdinalIgnoreCase) || Aquarium.SpeciesInfo.Contains("Florarium", StringComparison.OrdinalIgnoreCase)) Aquarium.TankTypeSubtitle = LocalizationService.Instance["TankType_Florarium"];
                else if (Aquarium.SpeciesInfo.Contains("Формикарий", StringComparison.OrdinalIgnoreCase) || Aquarium.SpeciesInfo.Contains("Formicarium", StringComparison.OrdinalIgnoreCase)) Aquarium.TankTypeSubtitle = LocalizationService.Instance["TankType_Formicarium"];
                else Aquarium.TankTypeSubtitle = LocalizationService.Instance["TankType_Aquarium"];
            }

            if ((Aquarium.Name.Equals("Мой первый аквариум", StringComparison.OrdinalIgnoreCase) || Aquarium.Name.Equals("My First Aquarium", StringComparison.OrdinalIgnoreCase)) &&
                Aquarium.TankTypeSubtitle != LocalizationService.Instance["TankType_Aquarium"])
            {
                Aquarium.Name = LocalizationService.Instance.IsEnglish
                    ? $"My First {Aquarium.TankTypeSubtitle}"
                    : $"Мой первый {Aquarium.TankTypeSubtitle.ToLowerInvariant()}";
            }
        }

        var sList = await _aquariumService.GetSensorsAsync(id);
        Sensors.Clear();
        foreach (var s in sList) Sensors.Add(s);
        HasNoSensors = Sensors.Count == 0;

        var rList = await _aquariumService.GetRelaysAsync(id);
        Relays.Clear();
        foreach (var r in rList) Relays.Add(r);
        HasNoRelays = Relays.Count == 0;

        var scList = await _aquariumService.GetScenariosAsync(id);
        Scenarios.Clear();
        foreach (var sc in scList) Scenarios.Add(sc);
        HasNoScenarios = Scenarios.Count == 0;

        await RefreshVacationModeAsync(id);
        await RefreshMaintenanceLogsAsync(id);
        await RefreshSchedulesAsync(id);

        IsBusy = false;
    }

    private bool _isUpdatingVacationModeInternally;

    async partial void OnIsVacationModeActiveChanged(bool value)
    {
        if (_isUpdatingVacationModeInternally || Aquarium == null) return;

        if (ActiveVacationMode == null)
        {
            if (value)
            {
                await ConfigureVacationModeAsync();
            }
            return;
        }

        if (ActiveVacationMode.IsActive != value)
        {
            await _aquariumService.ToggleVacationModeAsync(ActiveVacationMode.Id);
            ActiveVacationMode = ActiveVacationMode with { IsActive = value };
            VacationModeText = value
                ? string.Format(LocalizationService.Instance["Tank_VacationActive"], ActiveVacationMode.DateRange, ActiveVacationMode.CalculatedFeed)
                : LocalizationService.Instance["Tank_VacationDisabled"];
        }
    }

    private async Task RefreshVacationModeAsync(Guid ecosystemId)
    {
        try
        {
            _isUpdatingVacationModeInternally = true;
            var vacations = await _aquariumService.GetVacationModesAsync(ecosystemId);
            ActiveVacationMode = vacations.FirstOrDefault(x => x.IsActive) ?? vacations.FirstOrDefault();
            IsVacationModeActive = ActiveVacationMode?.IsActive ?? false;
            VacationModeText = IsVacationModeActive
                ? string.Format(LocalizationService.Instance["Tank_VacationActive"], ActiveVacationMode?.DateRange, ActiveVacationMode?.CalculatedFeed ?? 0)
                : LocalizationService.Instance["Tank_VacationShortDisabled"];
        }
        catch
        {
        }
        finally
        {
            _isUpdatingVacationModeInternally = false;
        }
    }

    private async Task RefreshMaintenanceLogsAsync(Guid ecosystemId)
    {
        try
        {
            var logs = await _aquariumService.GetMaintenanceLogsAsync(ecosystemId);
            MaintenanceLogs.Clear();
            foreach (var l in logs) MaintenanceLogs.Add(l);
            HasNoLogs = MaintenanceLogs.Count == 0;
        }
        catch
        {
        }
    }

    private async Task RefreshSchedulesAsync(Guid ecosystemId)
    {
        try
        {
            var scheds = await _aquariumService.GetSchedulesAsync(ecosystemId);
            Schedules.Clear();
            foreach (var s in scheds) Schedules.Add(s);
        }
        catch
        {
        }
    }

    [RelayCommand]
    private async Task ToggleScheduleAsync(ScheduleDto schedule)
    {
        var newState = !schedule.IsEnabled;
        var success = await _aquariumService.ToggleScheduleAsync(schedule.Id, newState);
        if (success)
        {
            var idx = Schedules.IndexOf(schedule);
            if (idx >= 0) Schedules[idx] = schedule with { IsEnabled = newState };
        }
    }

    [RelayCommand]
    private async Task AddScheduleAsync()
    {
        if (Aquarium == null || Relays.Count == 0)
        {
            await Shell.Current.DisplayAlert(LocalizationService.Instance["Dialog_ScheduleTitle"], LocalizationService.Instance["Dialog_ScheduleNoRelays"], LocalizationService.Instance["Common_Ok"]);
            return;
        }

        var relayNames = Relays.Select(r => r.Name).ToArray();
        var selectedRelayName = await Shell.Current.DisplayActionSheet(LocalizationService.Instance["Dialog_SelectRelayPrompt"], LocalizationService.Instance["Common_Cancel"], null, relayNames);
        if (string.IsNullOrWhiteSpace(selectedRelayName) || selectedRelayName == LocalizationService.Instance["Common_Cancel"] || selectedRelayName == "Отмена") return;

        var relay = Relays.FirstOrDefault(r => r.Name == selectedRelayName);
        if (relay == null) return;

        var startHourStr = await Shell.Current.DisplayPromptAsync(LocalizationService.Instance["Dialog_StartHourTitle"], LocalizationService.Instance["Dialog_StartHourMsg"], initialValue: "8", keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(startHourStr) || !int.TryParse(startHourStr, out var hour) || hour < 0 || hour > 23) return;

        var durStr = await Shell.Current.DisplayPromptAsync(LocalizationService.Instance["Dialog_DurationTitle"], LocalizationService.Instance["Dialog_DurationMsg"], initialValue: "480", keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(durStr) || !double.TryParse(durStr, out var dur) || dur <= 0) dur = 480;

        var cron = $"0 {hour} * * *";
        var request = new CreateScheduleRequestDto(
            Aquarium.Id,
            relay.Id,
            cron,
            dur,
            false,
            true
        );

        IsBusy = true;
        await _aquariumService.CreateScheduleAsync(request);
        await RefreshSchedulesAsync(Aquarium.Id);
        IsBusy = false;

        await Shell.Current.DisplayAlert(
            LocalizationService.Instance["Dialog_ScheduleTitle"],
            string.Format(LocalizationService.Instance["Dialog_ScheduleCreated"], relay.Name, hour, dur),
            LocalizationService.Instance["Common_Ok"]);
    }

    [RelayCommand]
    private async Task ToggleVacationModeAsync()
    {
        if (Aquarium == null) return;

        if (ActiveVacationMode == null)
        {
            await ConfigureVacationModeAsync();
            return;
        }

        var success = await _aquariumService.ToggleVacationModeAsync(ActiveVacationMode.Id);
        if (success)
        {
            IsVacationModeActive = !IsVacationModeActive;
            VacationModeText = IsVacationModeActive
                ? string.Format(LocalizationService.Instance["Tank_VacationActive"], ActiveVacationMode.DateRange, ActiveVacationMode.CalculatedFeed)
                : LocalizationService.Instance["Tank_VacationDisabled"];
        }
    }

    [RelayCommand]
    private async Task ConfigureVacationModeAsync()
    {
        if (Aquarium == null) return;

        var daysStr = await Shell.Current.DisplayPromptAsync(LocalizationService.Instance["Dialog_VacationDaysTitle"], LocalizationService.Instance["Dialog_VacationDaysMsg"], initialValue: "14", keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(daysStr) || !int.TryParse(daysStr, out var days) || days <= 0) return;

        var feedStr = await Shell.Current.DisplayPromptAsync(LocalizationService.Instance["Dialog_VacationFeedTitle"], LocalizationService.Instance["Dialog_VacationFeedMsg"], initialValue: "50", keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(feedStr) || !double.TryParse(feedStr, out var feedPct)) feedPct = 50.0;

        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(days);

        var request = new CreateVacationModeRequestDto(
            Aquarium.Id,
            start,
            end,
            true,
            feedPct
        );

        IsBusy = true;
        await _aquariumService.CreateVacationModeAsync(request);
        await RefreshVacationModeAsync(Aquarium.Id);
        IsBusy = false;

        await Shell.Current.DisplayAlert(
            LocalizationService.Instance["Dialog_VacationDaysTitle"],
            string.Format(LocalizationService.Instance["Dialog_VacationActivated"], days, end, feedPct),
            LocalizationService.Instance["Common_Ok"]);
    }

    [RelayCommand]
    private async Task QuickLogMaintenanceAsync(string? actionType)
    {
        if (Aquarium == null || string.IsNullOrWhiteSpace(actionType)) return;

        var displayTitle = actionType switch
        {
            "water_change" or "Подмена воды" or "Water Change" => LocalizationService.Instance["Tank_WaterChange"],
            "filter_clean" or "Чистка фильтра" or "Clean Filter" => LocalizationService.Instance["Tank_FilterClean"],
            "water_tests" or "Тестирование воды" or "Water Tests" => LocalizationService.Instance["Tank_WaterTests"],
            _ => LocalizationService.Instance["Tank_MaintenanceLog"]
        };

        var promptMsg = LocalizationService.Instance["Dialog_MaintenanceNotePrompt"];
        var note = await Shell.Current.DisplayPromptAsync(
            displayTitle,
            promptMsg,
            initialValue: actionType switch
            {
                "water_change" or "Подмена воды" or "Water Change" => LocalizationService.Instance.IsEnglish ? "Routine 20% RO water change" : "Плановая подмена 20% осмосом",
                "filter_clean" or "Чистка фильтра" or "Clean Filter" => LocalizationService.Instance.IsEnglish ? "Filter media rinsed" : "Промывка наполнителей фильтра",
                "water_tests" or "Тестирование воды" or "Water Tests" => "pH 6.8, TDS 140 ppm, NO3 10",
                _ => string.Empty
            });

        if (note == null) return;

        var metrics = ParseMetricsFromNote(note);
        var notesText = $"{displayTitle}: {note}".TrimEnd(':', ' ');
        if (notesText.Length > 1024) notesText = notesText[..1024];

        var request = new CreateMaintenanceLogRequestDto(
            Aquarium.Id,
            DateTime.UtcNow,
            metrics,
            notesText
        );

        IsBusy = true;
        await _aquariumService.AddMaintenanceLogAsync(request);
        await RefreshMaintenanceLogsAsync(Aquarium.Id);
        IsBusy = false;
    }

    private static Dictionary<string, double> ParseMetricsFromNote(string? note)
    {
        var dict = new Dictionary<string, double>();
        if (string.IsNullOrWhiteSpace(note)) return dict;

        var matches = System.Text.RegularExpressions.Regex.Matches(
            note,
            @"\b(?<key>pH|TDS|NO3|NO2|kH|gH|PO4|NH3|NH4|temp|temperature)\s*[:=]?\s*(?<val>\d+(?:[\.,]\d+)?)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var key = match.Groups["key"].Value;
            var valStr = match.Groups["val"].Value.Replace(',', '.');
            if (double.TryParse(valStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var val))
            {
                dict[key] = val;
            }
        }

        return dict;
    }

    [RelayCommand]
    private void SelectTab(object? parameter)
    {
        if (parameter is int idx)
        {
            SelectedTabIndex = idx;
        }
        else if (parameter is string s && int.TryParse(s, out var parsed))
        {
            SelectedTabIndex = parsed;
        }
    }

    [RelayCommand]
    private async Task ToggleRelayAsync(RelayModel relay)
    {
        var authorized = await _securityService.CheckRelayPinAsync(relay.Name);
        if (!authorized) return;

        await _aquariumService.ToggleRelayAsync(relay.Id);
        relay.IsOn = !relay.IsOn;
        var idx = Relays.IndexOf(relay);
        if (idx >= 0) Relays[idx] = relay;
    }

    [RelayCommand]
    private async Task ToggleScenarioAsync(ScenarioModel scenario)
    {
        await _aquariumService.ToggleScenarioAsync(scenario.Id);
        scenario.IsActive = !scenario.IsActive;
        var idx = Scenarios.IndexOf(scenario);
        if (idx >= 0) Scenarios[idx] = scenario;
    }

    [RelayCommand]
    private async Task AddSensorAsync()
    {
        await Shell.Current.GoToAsync($"AddSensorPage?aquariumId={Aquarium?.Id}");
    }

    [RelayCommand]
    private async Task AddRelayAsync()
    {
        await Shell.Current.GoToAsync($"AddRelayPage?aquariumId={Aquarium?.Id}");
    }

    [RelayCommand]
    private async Task AddScenarioAsync()
    {
        await Shell.Current.GoToAsync($"AddScenarioPage?aquariumId={Aquarium?.Id}");
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (Aquarium != null)
        {
            await Shell.Current.GoToAsync(nameof(EditAquariumPage), new Dictionary<string, object>
            {
                ["aquariumId"] = Aquarium.Id.ToString()
            });
        }
    }

    [RelayCommand]
    private async Task DeleteSensorAsync(SensorModel sensor)
    {
        if (sensor == null) return;
        var confirm = await Shell.Current.DisplayAlert(
            LocalizationService.Instance["Sensor_DeleteConfirmTitle"],
            string.Format(LocalizationService.Instance["Sensor_DeleteConfirmMsg"], sensor.Name),
            LocalizationService.Instance["Common_Delete"],
            LocalizationService.Instance["Common_Cancel"]);

        if (!confirm) return;

        await _aquariumService.DeleteSensorAsync(sensor.Id);
        Sensors.Remove(sensor);
        HasNoSensors = Sensors.Count == 0;
        if (Aquarium != null)
        {
            Aquarium.SensorsCount = Sensors.Count;
        }
    }

    [RelayCommand]
    private async Task DeleteRelayAsync(RelayModel relay)
    {
        if (relay == null) return;
        var confirm = await Shell.Current.DisplayAlert(
            LocalizationService.Instance["Relay_DeleteConfirmTitle"],
            string.Format(LocalizationService.Instance["Relay_DeleteConfirmMsg"], relay.Name),
            LocalizationService.Instance["Common_Delete"],
            LocalizationService.Instance["Common_Cancel"]);

        if (!confirm) return;

        await _aquariumService.DeleteRelayAsync(relay.Id);
        Relays.Remove(relay);
        HasNoRelays = Relays.Count == 0;
        if (Aquarium != null)
        {
            Aquarium.RelaysCount = Relays.Count;
        }
    }

    [RelayCommand]
    private async Task DeleteScenarioAsync(ScenarioModel scenario)
    {
        if (scenario == null) return;
        var confirm = await Shell.Current.DisplayAlert(
            LocalizationService.Instance["Scenario_DeleteConfirmTitle"],
            string.Format(LocalizationService.Instance["Scenario_DeleteConfirmMsg"], scenario.Name),
            LocalizationService.Instance["Common_Delete"],
            LocalizationService.Instance["Common_Cancel"]);

        if (!confirm) return;

        await _aquariumService.DeleteScenarioAsync(scenario.Id);
        Scenarios.Remove(scenario);
        HasNoScenarios = Scenarios.Count == 0;
        if (Aquarium != null)
        {
            Aquarium.ScenariosCount = Scenarios.Count;
        }
    }

    [RelayCommand]
    private async Task DeleteScheduleAsync(ScheduleDto schedule)
    {
        if (schedule == null) return;
        var confirm = await Shell.Current.DisplayAlert(
            LocalizationService.Instance["Dialog_DeleteScheduleTitle"],
            LocalizationService.Instance["Dialog_DeleteScheduleMsg"],
            LocalizationService.Instance["Common_Delete"],
            LocalizationService.Instance["Common_Cancel"]);

        if (!confirm) return;

        await _aquariumService.DeleteScheduleAsync(schedule.Id);
        Schedules.Remove(schedule);
    }

    [RelayCommand]
    private async Task DeleteMaintenanceLogAsync(MaintenanceLogDto log)
    {
        if (log == null) return;
        var confirm = await Shell.Current.DisplayAlert(
            LocalizationService.Instance["Log_DeleteConfirmTitle"],
            LocalizationService.Instance["Log_DeleteConfirmMsg"],
            LocalizationService.Instance["Common_Delete"],
            LocalizationService.Instance["Common_Cancel"]);

        if (!confirm) return;

        await _aquariumService.DeleteMaintenanceLogAsync(log.Id);
        MaintenanceLogs.Remove(log);
        HasNoLogs = MaintenanceLogs.Count == 0;
    }

    [RelayCommand]
    private async Task OpenNotificationsAsync()
    {
        await Shell.Current.GoToAsync(nameof(NotificationsPage));
    }
}
