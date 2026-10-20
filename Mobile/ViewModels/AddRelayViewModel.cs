using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

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
