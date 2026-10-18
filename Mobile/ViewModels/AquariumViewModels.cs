using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services;

namespace Mobile.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IAquariumService _aquariumService;
    private readonly IIdentityService _identityService;

    [ObservableProperty]
    private string _greeting = "Добрый день, Алексей!";

    [ObservableProperty]
    private string _systemStatusText = "Все аквасистемы в норме";

    [ObservableProperty]
    private bool _isAllNormal = true;

    [ObservableProperty]
    private bool _hasProblems = true;

    [ObservableProperty]
    private string _problemTitle = "Внимание: Требуется проверка";

    [ObservableProperty]
    private string _problemDescription = "Температура в аквариуме «Биотоп Танганьика №1» достигла 26.2 °C (выше целевой нормы на 0.2 °C).";

    [ObservableProperty]
    private ObservableCollection<AquariumModel> _aquariums = new();

    [ObservableProperty]
    private AquariumModel? _selectedAquarium;

    [ObservableProperty]
    private ObservableCollection<SensorModel> _activeSensors = new();

    [ObservableProperty]
    private ObservableCollection<RelayModel> _quickRelays = new();

    [ObservableProperty]
    private bool _isBusy;

    public DashboardViewModel(IAquariumService aquariumService, IIdentityService identityService)
    {
        _aquariumService = aquariumService;
        _identityService = identityService;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        var profile = await _identityService.GetCurrentUserAsync();
        Greeting = $"Добрый день, {profile.FullName.Split(' ')[0]}!";

        var list = await _aquariumService.GetAquariumsAsync();
        Aquariums.Clear();
        foreach (var item in list)
        {
            Aquariums.Add(item);
        }

        SelectedAquarium = Aquariums.FirstOrDefault();

        if (SelectedAquarium != null)
        {
            await LoadAquariumDetailsAsync(SelectedAquarium.Id);
        }

        IsBusy = false;
    }

    private async Task LoadAquariumDetailsAsync(Guid aquariumId)
    {
        var sensors = await _aquariumService.GetSensorsAsync(aquariumId);
        ActiveSensors.Clear();
        foreach (var s in sensors) ActiveSensors.Add(s);

        var relays = await _aquariumService.GetRelaysAsync(aquariumId);
        QuickRelays.Clear();
        foreach (var r in relays) QuickRelays.Add(r);
    }

    [RelayCommand]
    private async Task ToggleRelayAsync(RelayModel relay)
    {
        await _aquariumService.ToggleRelayAsync(relay.Id);
        // Refresh display state
        relay.IsOn = !relay.IsOn;
        var idx = QuickRelays.IndexOf(relay);
        if (idx >= 0)
        {
            QuickRelays[idx] = relay;
        }
    }

    [RelayCommand]
    private async Task SelectAquariumAsync(AquariumModel aquarium)
    {
        SelectedAquarium = aquarium;
        await LoadAquariumDetailsAsync(aquarium.Id);
    }

    [RelayCommand]
    private async Task GoToAquariumDetailsAsync(AquariumModel aquarium)
    {
        await Shell.Current.GoToAsync($"AquariumDetailsPage?aquariumId={aquarium.Id}");
    }

    [RelayCommand]
    private async Task AddAquariumAsync()
    {
        await Shell.Current.GoToAsync("AddAquariumPage");
    }

    [RelayCommand]
    private async Task AddSensorAsync()
    {
        await Shell.Current.GoToAsync("AddSensorPage");
    }

    [RelayCommand]
    private async Task AddRelayAsync()
    {
        await Shell.Current.GoToAsync("AddRelayPage");
    }
}

public partial class AquariumsViewModel : ObservableObject
{
    private readonly IAquariumService _aquariumService;

    [ObservableProperty]
    private ObservableCollection<AquariumModel> _aquariums = new();

    [ObservableProperty]
    private bool _isBusy;

    public AquariumsViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        var list = await _aquariumService.GetAquariumsAsync();
        Aquariums.Clear();
        foreach (var a in list) Aquariums.Add(a);
        IsBusy = false;
    }

    [RelayCommand]
    private async Task SelectAquariumAsync(AquariumModel aquarium)
    {
        await Shell.Current.GoToAsync($"AquariumDetailsPage?aquariumId={aquarium.Id}");
    }

    [RelayCommand]
    private async Task AddAquariumAsync()
    {
        await Shell.Current.GoToAsync("AddAquariumPage");
    }
}

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
}
