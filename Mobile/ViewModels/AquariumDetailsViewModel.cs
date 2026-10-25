using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private int _selectedTabIndex = 0; // 0: Sensors, 1: Relays, 2: Scenarios

    public bool IsSensorsTabSelected => SelectedTabIndex == 0;
    public bool IsRelaysTabSelected => SelectedTabIndex == 1;
    public bool IsScenariosTabSelected => SelectedTabIndex == 2;

    [ObservableProperty]
    private ObservableCollection<SensorModel> _sensors = new();

    [ObservableProperty]
    private ObservableCollection<RelayModel> _relays = new();

    [ObservableProperty]
    private ObservableCollection<ScenarioModel> _scenarios = new();

    [ObservableProperty]
    private bool _hasNoSensors;

    [ObservableProperty]
    private bool _hasNoRelays;

    [ObservableProperty]
    private bool _hasNoScenarios;

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
                else if (Aquarium.SpeciesInfo.Contains("Палюдариум", StringComparison.OrdinalIgnoreCase)) Aquarium.TankTypeSubtitle = "Палюдариум";
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
