using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services.Interfaces;

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
    private int _deviceTypeIndex = 0; // 0: Filter, 1: Heater, 2: Aerator, 3: Light, 4: Feeder

    [ObservableProperty]
    private int _protocolIndex = 0; // 0: Digital, 1: Analog, 2: I2C, 3: OneWire

    [ObservableProperty]
    private string _address = "GPIO 12";

    [ObservableProperty]
    private bool _isManualControl = true;

    [ObservableProperty]
    private bool _isOpenContact = true; // true: Открытый, false: Закрытый

    [ObservableProperty]
    private bool _isCancelDialogOpen;

    [ObservableProperty]
    private bool _isSuccessOpen;

    [ObservableProperty]
    private string _createdRelayName = string.Empty;

    [ObservableProperty]
    private string _createdRelaySubtitle = string.Empty;

    [ObservableProperty]
    private string _createdRelayIcon = "⚡";

    [ObservableProperty]
    private string _createdRelayStatus = "Открыто";

    public bool IsFormVisible => !IsSuccessOpen;

    public AddRelayViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;
    }

    [RelayCommand]
    private void SelectAddressPreset(string preset)
    {
        Address = preset;
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
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(RelayName))
        {
            await Shell.Current.DisplayAlert("Внимание", "Укажите название реле", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Address))
        {
            await Shell.Current.DisplayAlert("Внимание", "Укажите адрес подключения", "OK");
            return;
        }

        Guid.TryParse(AquariumIdString, out var aqId);

        var (type, icon, typeName) = DeviceTypeIndex switch
        {
            1 => (AquariumDeviceType.Heater, "🔥", "Нагреватель"),
            2 => (AquariumDeviceType.Aerator, "🫧", "Аэратор"),
            3 => (AquariumDeviceType.Lighting, "💡", "Освещение"),
            4 => (AquariumDeviceType.Feeder, "🍽️", "Автокормушка"),
            _ => (AquariumDeviceType.FilterPump, "⚡", "Фильтр")
        };

        var protocolName = ProtocolIndex switch
        {
            1 => "Analog",
            2 => "I2C",
            3 => "OneWire",
            _ => "Digital"
        };

        var modeText = IsManualControl ? "Ручной режим" : "Автоматический";
        int.TryParse(new string(Address.Where(char.IsDigit).ToArray()), out var channelNum);
        if (channelNum == 0) channelNum = 1;

        var relay = new RelayModel
        {
            AquariumId = aqId,
            Name = RelayName.Trim(),
            Type = type,
            PinChannel = channelNum,
            ScheduleInfo = $"{modeText} • {Address.Trim()}",
            IsOn = false
        };

        await _aquariumService.AddRelayAsync(relay);

        CreatedRelayName = RelayName.Trim();
        CreatedRelayIcon = icon;
        CreatedRelaySubtitle = $"{typeName} • {modeText} • {protocolName} • {Address.Trim()}";
        CreatedRelayStatus = IsOpenContact ? "Открыто" : "Закрыто";
        IsSuccessOpen = true;
        OnPropertyChanged(nameof(IsFormVisible));
    }

    [RelayCommand]
    private async Task ReturnToAquariumAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private void AddAnotherRelay()
    {
        RelayName = string.Empty;
        Address = "GPIO 12";
        IsSuccessOpen = false;
        IsCancelDialogOpen = false;
        OnPropertyChanged(nameof(IsFormVisible));
    }
}
