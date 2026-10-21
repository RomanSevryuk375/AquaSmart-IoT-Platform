using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IAquariumService _aquariumService;
    private readonly IIdentityService _identityService;

    [ObservableProperty]
    private string _greeting = "Добро пожаловать!";

    [ObservableProperty]
    private string _systemStatusText = "Все аквасистемы в норме";

    [ObservableProperty]
    private bool _isAllNormal = true;

    [ObservableProperty]
    private bool _hasProblems = false;

    [ObservableProperty]
    private string _problemTitle = string.Empty;

    [ObservableProperty]
    private string _problemDescription = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAquariums))]
    private bool _hasNoAquariums = false;

    public bool HasAquariums => !HasNoAquariums;

    [ObservableProperty]
    private ObservableCollection<AquariumModel> _aquariums = new();

    [ObservableProperty]
    private AquariumModel? _selectedAquarium;

    [ObservableProperty]
    private ObservableCollection<SensorModel> _activeSensors = new();

    [ObservableProperty]
    private ObservableCollection<DashboardEventModel> _recentEvents = new();

    public bool HasRecentEvents => RecentEvents.Count > 0;

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
        if (!string.IsNullOrWhiteSpace(profile.FullName))
        {
            var firstName = profile.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
            Greeting = $"Добрый день, {firstName}!";
        }
        else
        {
            Greeting = "Добро пожаловать!";
        }

        var list = await _aquariumService.GetAquariumsAsync();
        Aquariums.Clear();
        foreach (var item in list)
        {
            Aquariums.Add(item);
        }

        HasNoAquariums = Aquariums.Count == 0;
        SelectedAquarium = Aquariums.FirstOrDefault();

        RecentEvents.Clear();

        if (SelectedAquarium != null)
        {
            await LoadAquariumDetailsAsync(SelectedAquarium.Id);

            var offline = Aquariums.FirstOrDefault(a => !a.IsOnline);
            if (offline != null)
            {
                HasProblems = true;
                IsAllNormal = false;
                ProblemTitle = "Внимание: Требуется проверка";
                ProblemDescription = $"Ёмкость «{offline.Name}» не на связи с контроллером.";
                SystemStatusText = "Требуется внимание";
            }
            else
            {
                HasProblems = false;
                IsAllNormal = true;
                ProblemTitle = string.Empty;
                ProblemDescription = string.Empty;
                SystemStatusText = "Все аквасистемы в норме";
            }

            foreach (var aq in Aquariums)
            {
                if (!aq.IsOnline)
                {
                    RecentEvents.Add(new DashboardEventModel
                    {
                        TimeText = DateTime.Now.ToString("HH:mm"),
                        Message = $"⚠️ Ёмкость «{aq.Name}»: устройство не на связи",
                        IsWarning = true
                    });
                }
                else
                {
                    RecentEvents.Add(new DashboardEventModel
                    {
                        TimeText = DateTime.Now.ToString("HH:mm"),
                        Message = $"Ёмкость «{aq.Name}»: данные телеметрии обновлены ({aq.TemperatureDisplay})",
                        IsWarning = false
                    });
                }
            }
        }
        else
        {
            ActiveSensors.Clear();
            QuickRelays.Clear();
            HasProblems = false;
            IsAllNormal = true;
            ProblemTitle = string.Empty;
            ProblemDescription = string.Empty;
            SystemStatusText = "Подключите вашу первую ёмкость";
        }

        OnPropertyChanged(nameof(HasRecentEvents));
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
    private async Task AddControllerAsync()
    {
        await Shell.Current.GoToAsync("AddControllerPage");
    }

    [RelayCommand]
    private async Task GoToKnowledgeBaseAsync()
    {
        await Shell.Current.GoToAsync("//MainTabs/KnowledgeBasePage");
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
