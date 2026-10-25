using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services.Dto.Control;
using Mobile.Services.Dto.Notification;
using Mobile.Views;

namespace Mobile.ViewModels;

[QueryProperty(nameof(AquariumIdString), "aquariumId")]
public partial class AquariumDetailsViewModel : ObservableObject
{
    private readonly IAquariumService _aquariumService;

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
    private VacationModeDto? _activeVacationMode;

    [ObservableProperty]
    private bool _isVacationModeActive;

    [ObservableProperty]
    private string _vacationModeText = "Режим отпуска отключен";

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

    public AquariumDetailsViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;
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
        Aquarium = await _aquariumService.GetAquariumByIdAsync(id);
        if (Aquarium != null)
        {
            if (string.IsNullOrWhiteSpace(Aquarium.TankTypeSubtitle) || Aquarium.TankTypeSubtitle == "Ёмкость")
            {
                if (Aquarium.SpeciesInfo.Contains("Террариум", StringComparison.OrdinalIgnoreCase)) Aquarium.TankTypeSubtitle = "Террариум";
                else if (Aquarium.SpeciesInfo.Contains("Гроубокс", StringComparison.OrdinalIgnoreCase)) Aquarium.TankTypeSubtitle = "Гроубокс";
                else if (Aquarium.SpeciesInfo.Contains("Флорариум", StringComparison.OrdinalIgnoreCase)) Aquarium.TankTypeSubtitle = "Флорариум";
                else if (Aquarium.SpeciesInfo.Contains("Формикарий", StringComparison.OrdinalIgnoreCase)) Aquarium.TankTypeSubtitle = "Формикарий";
                else Aquarium.TankTypeSubtitle = "Аквариум";
            }

            if (Aquarium.Name.Equals("Мой первый аквариум", StringComparison.OrdinalIgnoreCase) &&
                Aquarium.TankTypeSubtitle != "Аквариум")
            {
                Aquarium.Name = $"Мой первый {Aquarium.TankTypeSubtitle.ToLowerInvariant()}";
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
                ? $"Активен: {ActiveVacationMode.DateRange} (корм {ActiveVacationMode.CalculatedFeed:F0}%)"
                : "Режим отпуска отключен";
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
                ? $"Активен: {ActiveVacationMode?.DateRange} (корм {ActiveVacationMode?.CalculatedFeed:F0}%)"
                : "Режим отпуска отключен";
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
                ? $"Активен: {ActiveVacationMode.DateRange} (корм {ActiveVacationMode.CalculatedFeed:F0}%)"
                : "Режим отпуска отключен";
        }
    }

    [RelayCommand]
    private async Task ConfigureVacationModeAsync()
    {
        if (Aquarium == null) return;

        var daysStr = await Shell.Current.DisplayPromptAsync("Режим отпуска", "Количество дней отпуска:", initialValue: "14", keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(daysStr) || !int.TryParse(daysStr, out var days) || days <= 0) return;

        var feedStr = await Shell.Current.DisplayPromptAsync("Режим отпуска", "Снижение корма (в % от нормы, например 50):", initialValue: "50", keyboard: Keyboard.Numeric);
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

        await Shell.Current.DisplayAlert("Режим отпуска", $"Режим отпуска активирован на {days} дн. (до {end:dd.MM.yyyy}). Кормление снижено до {feedPct}%.", "OK");
    }

    [RelayCommand]
    private async Task QuickLogMaintenanceAsync(string? actionType)
    {
        if (Aquarium == null || string.IsNullOrWhiteSpace(actionType)) return;

        var note = await Shell.Current.DisplayPromptAsync(
            actionType,
            "Добавьте комментарий (например, 'Заменено 20% воды' или 'Промыт губчатый фильтр'):",
            initialValue: actionType switch
            {
                "Подмена воды" => "Плановая подмена 20% осмосом",
                "Чистка фильтра" => "Промывка наполнителей фильтра",
                "Тестирование воды" => "pH 6.8, TDS 140 ppm, NO3 10",
                _ => string.Empty
            });

        if (note == null) return;

        var request = new CreateMaintenanceLogRequestDto(
            Aquarium.Id,
            DateTime.UtcNow,
            null,
            $"{actionType}: {note}".TrimEnd(':', ' ')
        );

        IsBusy = true;
        await _aquariumService.AddMaintenanceLogAsync(request);
        await RefreshMaintenanceLogsAsync(Aquarium.Id);
        IsBusy = false;
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
            await Shell.Current.GoToAsync($"{nameof(EditAquariumPage)}?aquariumId={Aquarium.Id}");
        }
    }

    [RelayCommand]
    private async Task OpenNotificationsAsync()
    {
        await Shell.Current.GoToAsync(nameof(NotificationsPage));
    }
}
