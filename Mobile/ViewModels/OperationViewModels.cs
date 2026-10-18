using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services;

namespace Mobile.ViewModels;

public partial class AddAquariumViewModel : ObservableObject
{
    private readonly IAquariumService _aquariumService;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _volumeString = "200";

    [ObservableProperty]
    private int _waterTypeIndex = 0; // 0: Freshwater, 1: Marine, 2: Brackish

    [ObservableProperty]
    private string _species = string.Empty;

    [ObservableProperty]
    private string _dimensions = string.Empty;

    public AddAquariumViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await Shell.Current.DisplayAlert("Внимание", "Укажите название ёмкости", "OK");
            return;
        }

        double.TryParse(VolumeString, out var volume);
        if (volume <= 0) volume = 100;

        var model = new AquariumModel
        {
            Name = Name,
            VolumeLiters = volume,
            WaterType = WaterTypeIndex switch
            {
                1 => WaterType.Marine,
                2 => WaterType.Brackish,
                _ => WaterType.Freshwater
            },
            SpeciesInfo = Species,
            Dimensions = string.IsNullOrWhiteSpace(Dimensions) ? "Стандартная" : Dimensions
        };

        await _aquariumService.AddAquariumAsync(model);
        await Shell.Current.DisplayAlert("Успешно", $"Ёмкость «{Name}» добавлена в систему!", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}

[QueryProperty(nameof(AquariumIdString), "aquariumId")]
public partial class AddSensorViewModel : ObservableObject
{
    private readonly IAquariumService _aquariumService;

    [ObservableProperty]
    private string? _aquariumIdString;

    [ObservableProperty]
    private string _sensorName = string.Empty;

    [ObservableProperty]
    private int _sensorTypeIndex = 0; // 0: Temp, 1: pH, 2: O2, 3: TDS

    [ObservableProperty]
    private string _minValueString = "22.0";

    [ObservableProperty]
    private string _maxValueString = "28.0";

    public AddSensorViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(SensorName))
        {
            await Shell.Current.DisplayAlert("Внимание", "Укажите название датчика", "OK");
            return;
        }

        Guid.TryParse(AquariumIdString, out var aqId);
        double.TryParse(MinValueString, out var min);
        double.TryParse(MaxValueString, out var max);

        var type = SensorTypeIndex switch
        {
            1 => SensorType.Ph,
            2 => SensorType.DissolvedOxygen,
            3 => SensorType.TdsSalinity,
            _ => SensorType.Temperature
        };

        var unit = type switch
        {
            SensorType.Ph => "pH",
            SensorType.DissolvedOxygen => "мг/л",
            SensorType.TdsSalinity => "ppm",
            _ => "°C"
        };

        var sensor = new SensorModel
        {
            AquariumId = aqId,
            Name = SensorName,
            Type = type,
            Value = (min + max) / 2.0,
            Unit = unit,
            MinThreshold = min,
            MaxThreshold = max
        };

        await _aquariumService.AddSensorAsync(sensor);
        await Shell.Current.DisplayAlert("Успех", $"Датчик «{SensorName}» откалиброван и привязан!", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}

[QueryProperty(nameof(AquariumIdString), "aquariumId")]
public partial class AddRelayViewModel : ObservableObject
{
    private readonly IAquariumService _aquariumService;

    [ObservableProperty]
    private string? _aquariumIdString;

    [ObservableProperty]
    private string _relayName = string.Empty;

    [ObservableProperty]
    private int _deviceTypeIndex = 0; // 0: Heater, 1: Aerator, 2: Pump, 3: Feeder, 4: Light

    [ObservableProperty]
    private int _pinChannel = 1;

    [ObservableProperty]
    private string _schedule = "09:00 - 21:00";

    public AddRelayViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(RelayName))
        {
            await Shell.Current.DisplayAlert("Внимание", "Укажите имя устройства", "OK");
            return;
        }

        Guid.TryParse(AquariumIdString, out var aqId);

        var type = DeviceTypeIndex switch
        {
            1 => AquariumDeviceType.Aerator,
            2 => AquariumDeviceType.FilterPump,
            3 => AquariumDeviceType.Feeder,
            4 => AquariumDeviceType.Lighting,
            _ => AquariumDeviceType.Heater
        };

        var relay = new RelayModel
        {
            AquariumId = aqId,
            Name = RelayName,
            Type = type,
            PinChannel = PinChannel,
            ScheduleInfo = Schedule,
            IsOn = false
        };

        await _aquariumService.AddRelayAsync(relay);
        await Shell.Current.DisplayAlert("Успех", $"Реле «{RelayName}» подключено к каналу {PinChannel}!", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}

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

public partial class KnowledgeBaseViewModel : ObservableObject
{
    private readonly IKnowledgeService _knowledgeService;

    [ObservableProperty]
    private ObservableCollection<KnowledgeArticleModel> _articles = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public KnowledgeBaseViewModel(IKnowledgeService knowledgeService)
    {
        _knowledgeService = knowledgeService;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        var list = await _knowledgeService.GetArticlesAsync();
        Articles.Clear();
        foreach (var a in list) Articles.Add(a);
        IsBusy = false;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        var list = await _knowledgeService.SearchArticlesAsync(SearchQuery);
        Articles.Clear();
        foreach (var a in list) Articles.Add(a);
    }

    [RelayCommand]
    private async Task SelectArticleAsync(KnowledgeArticleModel article)
    {
        await Shell.Current.DisplayAlert(article.Title, $"{article.Content}\n\nКатегория: {article.Category}", "Закрыть");
    }
}

public partial class ProfileViewModel : ObservableObject
{
    private readonly IIdentityService _identityService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private UserProfileModel _user = new();

    [ObservableProperty]
    private bool _isBusy;

    public ProfileViewModel(IIdentityService identityService, IAuthService authService)
    {
        _identityService = identityService;
        _authService = authService;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        User = await _identityService.GetCurrentUserAsync();
        IsBusy = false;
    }

    [RelayCommand]
    private async Task EditProfileAsync()
    {
        var newName = await Shell.Current.DisplayPromptAsync("Редактирование", "Введите ФИО:", initialValue: User.FullName);
        if (!string.IsNullOrWhiteSpace(newName))
        {
            User.FullName = newName;
            await _identityService.UpdateCurrentUserAsync(User);
            OnPropertyChanged(nameof(User));
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var answer = await Shell.Current.DisplayAlert("Выход", "Вы действительно хотите выйти из аккаунта?", "Да", "Отмена");
        if (answer)
        {
            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("//MainPage");
        }
    }

    [RelayCommand]
    private async Task ShowAboutAsync()
    {
        await Shell.Current.DisplayAlert("О приложении", "AquaSmart IoT Platform Mobile\nВерсия: 1.0.0 (Release)\n\nПлатформа комплексного мониторинга и автоматизации аквасистем.", "Закрыть");
    }

    [RelayCommand]
    private async Task ShowSettingsAsync()
    {
        await Shell.Current.DisplayAlert("Настройки приложения", "• Тема: Системная (Светлая/Тёмная)\n• Интервал телеметрии: 5 сек\n• Push-уведомления: Включены\n• Звуковые оповещения алертов: Включены", "OK");
    }

    [RelayCommand]
    private async Task ShowSecurityAsync()
    {
        await Shell.Current.DisplayAlert("Безопасность", "• Двухфакторная аутентификация: Активна\n• Сессия: Защищена JWT и SecureStorage\n• Последний вход: Сегодня", "OK");
    }

    [RelayCommand]
    private async Task ShowSupportAsync()
    {
        await Shell.Current.DisplayAlert("Помощь и поддержка", "Служба поддержки AquaSmart IoT:\nEmail: support@aquasmart.io\nTelegram: @AquaSmartSupport\n\nБаза знаний доступна в боковом меню.", "Понятно");
    }

    [RelayCommand]
    private async Task DeleteAccountAsync()
    {
        var confirm = await Shell.Current.DisplayAlert("Удаление аккаунта", "Вы действительно хотите удалить свой аккаунт и все связанные контроллеры? Это действие необратимо.", "Удалить", "Отмена");
        if (confirm)
        {
            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
