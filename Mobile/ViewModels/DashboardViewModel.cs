using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
